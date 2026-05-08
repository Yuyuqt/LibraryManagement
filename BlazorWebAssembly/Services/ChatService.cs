using System.Net.Http.Json;
using Blazored.LocalStorage;
using LibraryManagement.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorWebAssembly.Services;

public class ChatService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly LibraryApiClient _apiClient;
    private const string BaseHistoryKey = "library_chat_history";
    private const string ApiUrl = "https://yuruyuruu-fastapi.hf.space/chat";

    public ChatService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider, LibraryApiClient apiClient)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
        _apiClient = apiClient;
    }

    private async Task<string> GetUserHistoryKeyAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var userId = user.Identity?.IsAuthenticated == true ? user.Identity.Name : "guest";
        return $"{BaseHistoryKey}_{userId}";
    }

    public async Task<List<ChatMessage>> GetHistoryAsync()
    {
        try
        {
            var key = await GetUserHistoryKeyAsync();
            var history = await _localStorage.GetItemAsync<List<ChatMessage>>(key);
            return history ?? new List<ChatMessage>();
        }
        catch
        {
            return new List<ChatMessage>();
        }
    }

    public async Task SaveHistoryAsync(List<ChatMessage> history)
    {
        var key = await GetUserHistoryKeyAsync();
        await _localStorage.SetItemAsync(key, history);
    }

    public async Task ClearHistoryAsync()
    {
        var key = await GetUserHistoryKeyAsync();
        await _localStorage.RemoveItemAsync(key);
    }

    public async Task<ChatResponse?> SendMessageAsync(string message, List<ChatMessage> history)
    {
        try
        {
            // 1. Fetch System Data (Plans)
            var memberships = await _apiClient.GetMembershipsAsync();
            var plansContext = string.Join("\n", memberships.Select(m => $"- {m.Type} Plan: {m.Price} MMK, {m.MaxBooks} books max, {m.BorrowingDays} days borrowing."));
            
            // 2. Fetch User Personal Data (Wallet, Loyalty, Sub)
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var isAuth = authState.User.Identity?.IsAuthenticated == true;
            var userName = authState.User.Identity?.Name ?? "Guest";
            
            string userContext = $"User Name: {userName}\nStatus: {(isAuth ? "Logged In" : "Guest")}\n";
            
            if (isAuth)
            {
                var wallet = await _apiClient.GetWalletBalanceAsync();
                var loyalty = await _apiClient.GetMyLoyaltyAccountAsync();
                var sub = await _apiClient.GetMySubscriptionAsync();
                
                userContext += $"- Wallet Balance: {wallet:N0} MMK\n";
                userContext += $"- Loyalty Points: {loyalty?.CurrentBalance ?? 0}\n";
                userContext += $"- Current Subscription: {sub?.MembershipType ?? "None"} (Status: {sub?.Status ?? "N/A"})\n";
            }

            var contextPrompt = $"{userContext}\n\nActual Library Plans:\n{plansContext}\n\nUser Question: {message}";

            using var client = new HttpClient();
            
            var request = new ChatRequest
            {
                Message = contextPrompt,
                History = history.Select(m => new ChatHistoryItem 
                { 
                    Role = m.Role, 
                    Content = m.Content 
                }).ToList()
            };

            var response = await client.PostAsJsonAsync(ApiUrl, request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ChatResponse>();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"AI API Error ({response.StatusCode}): {errorContent}");
            
            return new ChatResponse { Response = "I'm having a little trouble connecting to the library archives. Try again in a moment!" };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chat Service Exception: {ex.Message}");
            return new ChatResponse { Response = "Connection failed. Please check your internet or the AI service status." };
        }
    }
}

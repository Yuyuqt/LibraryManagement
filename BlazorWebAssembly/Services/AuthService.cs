using System.Net.Http.Json;
using Blazored.LocalStorage;
using LibraryManagement.Shared.Models;
using BlazorWebAssembly.Providers;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorWebAssembly.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            
            if (!response.IsSuccessStatusCode)
                return false;

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            
            if (authResponse != null)
            {
                await _localStorage.SetItemAsync("authToken", authResponse.Token);
                ((JwtAuthenticationStateProvider)_authStateProvider).NotifyUserAuthentication(authResponse.Token);
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", authResponse.Token);
                return true;
            }

            return false;
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            
            if (!response.IsSuccessStatusCode)
                return false;

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (authResponse != null)
            {
                // Auto login after registration
                await _localStorage.SetItemAsync("authToken", authResponse.Token);
                ((JwtAuthenticationStateProvider)_authStateProvider).NotifyUserAuthentication(authResponse.Token);
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", authResponse.Token);
                return true;
            }

            return false;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((JwtAuthenticationStateProvider)_authStateProvider).NotifyUserLogout();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}

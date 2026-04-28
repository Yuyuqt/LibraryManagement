using LibraryManagement.Shared.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Text.Json;

namespace BlazorFrontend.Services;

public class WishlistService
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly LibraryApiClient _apiClient;
    private const string WishlistKey = "user_wishlist";
    private const string FavouriteKey = "user_favourites";

    public WishlistService(ProtectedLocalStorage localStorage, LibraryApiClient apiClient)
    {
        _localStorage = localStorage;
        _apiClient = apiClient;
    }

    public async Task SyncCurrentWishlistAsync()
    {
        var list = await GetWishlistAsync();
        await SyncWithServerAsync(list);
    }

    private async Task SyncWithServerAsync(List<BookDto> list)
    {
        try
        {
            var bookIds = list.Select(b => b.Id).ToList();
            await _apiClient.SyncWishlistAsync(bookIds);
        }
        catch { }
    }

    public async Task<List<BookDto>> GetWishlistAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(WishlistKey);
            if (result.Success && !string.IsNullOrWhiteSpace(result.Value))
            {
                var localList = JsonSerializer.Deserialize<List<BookDto>>(result.Value) ?? new List<BookDto>();
                if (!localList.Any()) return localList;

                // Refresh from server
                var latestList = await _apiClient.GetBooksByIdsAsync(localList.Select(b => b.Id).ToList());
                if (latestList.Any())
                {
                    // Update local storage with fresh data
                    await _localStorage.SetAsync(WishlistKey, JsonSerializer.Serialize(latestList));
                    return latestList;
                }
                return localList;
            }
        }
        catch { }
        return new List<BookDto>();
    }

    public async Task AddToWishlistAsync(BookDto book)
    {
        var list = await GetWishlistAsync();
        if (!list.Any(b => b.Id == book.Id))
        {
            list.Add(book);
            await _localStorage.SetAsync(WishlistKey, JsonSerializer.Serialize(list));
            await SyncWithServerAsync(list);
        }
    }

    public async Task RemoveFromWishlistAsync(int bookId)
    {
        var list = await GetWishlistAsync();
        var item = list.FirstOrDefault(b => b.Id == bookId);
        if (item != null)
        {
            list.Remove(item);
            await _localStorage.SetAsync(WishlistKey, JsonSerializer.Serialize(list));
            await SyncWithServerAsync(list);
        }
    }

    public async Task<List<BookDto>> GetFavouritesAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(FavouriteKey);
            if (result.Success && !string.IsNullOrWhiteSpace(result.Value))
            {
                var localList = JsonSerializer.Deserialize<List<BookDto>>(result.Value) ?? new List<BookDto>();
                if (!localList.Any()) return localList;

                // Refresh from server
                var latestList = await _apiClient.GetBooksByIdsAsync(localList.Select(b => b.Id).ToList());
                if (latestList.Any())
                {
                    // Update local storage with fresh data
                    await _localStorage.SetAsync(FavouriteKey, JsonSerializer.Serialize(latestList));
                    return latestList;
                }
                return localList;
            }
        }
        catch { }
        return new List<BookDto>();
    }

    public async Task AddToFavouritesAsync(BookDto book)
    {
        var list = await GetFavouritesAsync();
        if (!list.Any(b => b.Id == book.Id))
        {
            list.Add(book);
            await _localStorage.SetAsync(FavouriteKey, JsonSerializer.Serialize(list));
        }
    }

    public async Task RemoveFromFavouriteAsync(int bookId)
    {
        var list = await GetFavouritesAsync();
        var item = list.FirstOrDefault(b => b.Id == bookId);
        if (item != null)
        {
            list.Remove(item);
            await _localStorage.SetAsync(FavouriteKey, JsonSerializer.Serialize(list));
        }
    }
}

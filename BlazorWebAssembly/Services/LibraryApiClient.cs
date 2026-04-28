using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LibraryManagement.Shared.Models;

namespace BlazorWebAssembly.Services
{
    public class LibraryApiClient
    {
        private readonly HttpClient _httpClient;

        public LibraryApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #region Books
        public async Task<IEnumerable<BookDto>> GetBooksAsync(int? categoryId = null)
        {
            var url = categoryId.HasValue ? $"api/books?categoryId={categoryId}" : "api/books";
            return await _httpClient.GetFromJsonAsync<IEnumerable<BookDto>>(url) ?? Enumerable.Empty<BookDto>();
        }

        public async Task<BookDto?> GetBookAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<BookDto>($"api/books/{id}");
        }

        public async Task<BookDto?> CreateBookAsync(BookCreateRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/books", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<BookDto>() : null;
        }

        public async Task<BookDto?> UpdateBookAsync(int id, BookUpdateRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/books/{id}", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<BookDto>() : null;
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/books/{id}");
            return response.IsSuccessStatusCode;
        }
        #endregion

        #region Borrowings
        public async Task<BorrowingDto?> BorrowBookAsync(BorrowRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/borrowings/borrow", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<BorrowingDto>() : null;
        }

        public async Task<BorrowingDto?> ReturnBookAsync(Guid id)
        {
            var response = await _httpClient.PostAsync($"api/borrowings/return/{id}", null);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<BorrowingDto>() : null;
        }

        public async Task<IEnumerable<BorrowingDto>> GetMyBorrowingsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<BorrowingDto>>("api/borrowings/me") ?? Enumerable.Empty<BorrowingDto>();
        }

        public async Task<IEnumerable<BorrowingDto>> GetAllBorrowingsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<BorrowingDto>>("api/borrowings") ?? Enumerable.Empty<BorrowingDto>();
        }

        public async Task<bool> RequestReturnAsync(Guid borrowingId)
        {
            var response = await _httpClient.PostAsync($"api/borrowings/return-request/{borrowingId}", null);
            return response.IsSuccessStatusCode;
        }
        #endregion

        #region Categories
        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<CategoryDto>>("api/categories") ?? Enumerable.Empty<CategoryDto>();
        }
        #endregion

        #region Subscriptions
        public async Task<IEnumerable<MembershipDto>> GetMembershipsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<MembershipDto>>("api/memberships") ?? Enumerable.Empty<MembershipDto>();
        }

        public async Task<SubscriptionDto?> GetMySubscriptionAsync()
        {
            try { return await _httpClient.GetFromJsonAsync<SubscriptionDto>("api/subscriptions/me"); }
            catch { return null; }
        }

        public async Task<SubscriptionDto?> SubscribeAsync(SubscribeRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/subscriptions/subscribe", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<SubscriptionDto>() : null;
        }
        #endregion

        #region Loyalty
        public async Task<LoyaltyAccountDto?> GetMyLoyaltyAccountAsync()
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString };
                return await _httpClient.GetFromJsonAsync<LoyaltyAccountDto>("api/Loyalty/my-account", options);
            }
            catch { return null; }
        }

        public async Task<IEnumerable<LoyaltyRewardDto>> GetActiveRewardsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<LoyaltyRewardDto>>("api/Loyalty/rewards") ?? Enumerable.Empty<LoyaltyRewardDto>();
        }

        public async Task<(bool Success, string Message)> ClaimRewardAsync(string rewardId, string? notes = null)
        {
            var request = new ClaimRewardRequestDto { RewardId = rewardId, Notes = notes };
            var response = await _httpClient.PostAsJsonAsync("api/Loyalty/claim", request);
            
            if (response.IsSuccessStatusCode) return (true, "Claimed successfully!");
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
        #endregion
    }
}

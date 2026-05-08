using System.Text.Json.Serialization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;
using LibraryManagement.Shared.Models;
using Blazored.LocalStorage;


namespace BlazorWebAssembly.Services
{
    public class LibraryApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public LibraryApiClient(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        /// <summary>Ensures the JWT token from localStorage is attached before every request.</summary>
        private async Task EnsureAuthHeader()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrWhiteSpace(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
        }


        #region Auth
        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<AuthResponse>() : null;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<AuthResponse>() : null;
        }

        public async Task<object?> GetProfileAsync()
        {
            return await _httpClient.GetFromJsonAsync<object>("api/auth/me");
        }
        #endregion

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

        public async Task<List<BookDto>> GetBooksByIdsAsync(List<int> ids)
        {
            if (ids == null || !ids.Any()) return new List<BookDto>();
            var idsStr = string.Join(",", ids);
            return await _httpClient.GetFromJsonAsync<List<BookDto>>($"api/books/by-ids?ids={idsStr}") ?? new List<BookDto>();
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
        #endregion

        #region Categories
        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<CategoryDto>>("api/categories") ?? Enumerable.Empty<CategoryDto>();
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesWithBooksAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<CategoryDto>>("api/categories/with-books") ?? Enumerable.Empty<CategoryDto>();
        }

        public async Task<CategoryDto?> CreateCategoryAsync(CategoryCreateRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/categories", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<CategoryDto>() : null;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/categories/{id}");
            return response.IsSuccessStatusCode;
        }
        #endregion

        #region Subscriptions
        public async Task<IEnumerable<MembershipDto>> GetMembershipsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<MembershipDto>>("api/memberships") ?? Enumerable.Empty<MembershipDto>();
        }

        public async Task<SubscriptionDto?> GetMySubscriptionAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<SubscriptionDto>("api/subscriptions/me");
            }
            catch { return null; }
        }

        public async Task<IEnumerable<SubscriptionDto>> GetMyAllSubscriptionsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<IEnumerable<SubscriptionDto>>("api/subscriptions/me/all") ?? Enumerable.Empty<SubscriptionDto>();
            }
            catch { return Enumerable.Empty<SubscriptionDto>(); }
        }

        public async Task<SubscriptionDto?> SubscribeAsync(SubscribeRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/subscriptions/subscribe", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<SubscriptionDto>() : null;
        }

        public async Task<SubscriptionDto?> GetUserSubscriptionAsync(Guid userId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<SubscriptionDto>($"api/subscriptions/user/{userId}");
            }
            catch { return null; }
        }

        public async Task<SubscriptionDto?> AdminSubscribeAsync(AdminSubscribeRequest request)
        {
            await EnsureAuthHeader();
            var response = await _httpClient.PostAsJsonAsync("api/subscriptions/admin-subscribe", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<SubscriptionDto>() : null;
        }


        public async Task<IEnumerable<SubscriptionDto>> GetAllSubscriptionsAsync()
        {
            await EnsureAuthHeader();
            return await _httpClient.GetFromJsonAsync<IEnumerable<SubscriptionDto>>("api/subscriptions/all") ?? Enumerable.Empty<SubscriptionDto>();
        }

        public async Task<IEnumerable<SubscriptionDto>> GetPendingSubscriptionsAsync()
        {
            await EnsureAuthHeader();
            return await _httpClient.GetFromJsonAsync<IEnumerable<SubscriptionDto>>("api/subscriptions/pending") ?? Enumerable.Empty<SubscriptionDto>();
        }

        public async Task<bool> ApproveSubscriptionAsync(ApproveSubscriptionRequest request)
        {
            await EnsureAuthHeader();
            var response = await _httpClient.PostAsJsonAsync("api/subscriptions/approve", request);
            return response.IsSuccessStatusCode;
        }


        public async Task<SubscriptionUpgradePreviewDto?> GetUpgradePreviewAsync(int membershipId)
        {
            try
            {
                await EnsureAuthHeader();
                return await _httpClient.GetFromJsonAsync<SubscriptionUpgradePreviewDto>($"api/subscriptions/preview-upgrade/{membershipId}");
            }
            catch { return null; }
        }

        public async Task<SubscriptionDto?> SubscribeWithWalletAsync(SubscribeRequest request)
        {
            await EnsureAuthHeader();
            var response = await _httpClient.PostAsJsonAsync("api/subscriptions/subscribe-wallet", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<SubscriptionDto>() : null;
        }
        #endregion

        #region Wallet
        public async Task<decimal> GetWalletBalanceAsync()
        {
            try
            {
                await EnsureAuthHeader();
                return await _httpClient.GetFromJsonAsync<decimal>("api/wallet/balance");
            }
            catch { return 0; }
        }

        public async Task<IEnumerable<WalletTransactionDto>> GetWalletHistoryAsync()
        {
            try
            {
                await EnsureAuthHeader();
                return await _httpClient.GetFromJsonAsync<IEnumerable<WalletTransactionDto>>("api/wallet/history") ?? Enumerable.Empty<WalletTransactionDto>();
            }
            catch { return Enumerable.Empty<WalletTransactionDto>(); }
        }


        public async Task<(bool Success, string Message)> TopUpWalletAsync(TopUpRequest request)
        {
            await EnsureAuthHeader();
            var response = await _httpClient.PostAsJsonAsync("api/wallet/topup", request);

            if (response.IsSuccessStatusCode) return (true, "Success");
            
            // Read as string first to avoid crash if response is not JSON (e.g. 401/403 with empty body)
            var raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw))
                return (false, $"HTTP {(int)response.StatusCode}: {response.StatusCode}");
            
            try
            {
                var error = JsonSerializer.Deserialize<JsonDocument>(raw);
                var msg = error?.RootElement.TryGetProperty("message", out var m) == true ? m.GetString() : raw;
                return (false, msg ?? raw);
            }
            catch
            {
                return (false, raw);
            }
        }
        #endregion



        public async Task<LoyaltyAccountDto?> GetMyLoyaltyAccountAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Loyalty/my-account");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString }; 
                    return await response.Content.ReadFromJsonAsync<LoyaltyAccountDto>(options);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<LoyaltyRedemptionDto>> GetMyRedemptionsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<IEnumerable<LoyaltyRedemptionDto>>("api/Loyalty/my-redemptions") ?? Enumerable.Empty<LoyaltyRedemptionDto>();
            }
            catch { return Enumerable.Empty<LoyaltyRedemptionDto>(); }
        }

        public async Task<IEnumerable<LoyaltyRedemptionDto>> GetMyPendingRedemptionsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<IEnumerable<LoyaltyRedemptionDto>>("api/Loyalty/my-pending-redemptions") ?? Enumerable.Empty<LoyaltyRedemptionDto>();
            }
            catch { return Enumerable.Empty<LoyaltyRedemptionDto>(); }
        }

        public async Task<IEnumerable<PointHistoryEntryDto>> GetPointsHistoryAsync(string accountId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<IEnumerable<PointHistoryEntryDto>>("api/Loyalty/my-points-history") ?? Enumerable.Empty<PointHistoryEntryDto>();
            }
            catch { return Enumerable.Empty<PointHistoryEntryDto>(); }
        }

        public async Task<IEnumerable<UserPointsHistoryDto>> GetAllMembersPointsHistoryAsync()
        {
            try {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString };
                var response = await _httpClient.GetAsync("api/Loyalty/admin/all-points-history");
                var raw = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return Enumerable.Empty<UserPointsHistoryDto>();

                var data = JsonSerializer.Deserialize<List<UserPointsHistoryDto>>(raw, options) ?? new List<UserPointsHistoryDto>();
                foreach (var member in data)
                {
                    member.History = member.History?.ToList() ?? new List<PointHistoryEntryDto>();
                    member.Redemptions = member.Redemptions?.ToList() ?? new List<LoyaltyRedemptionDto>();
                }
                return data;
            } catch { return Enumerable.Empty<UserPointsHistoryDto>(); }
        }

        public async Task<bool> RequestReturnAsync(Guid borrowingId)
        {
            var response = await _httpClient.PostAsync($"api/borrowings/return-request/{borrowingId}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<(bool Success, string Message)> ClaimRewardAsync(string rewardId, string? notes = null)
        {
            try
            {
                var request = new ClaimRewardRequestDto { RewardId = rewardId, Notes = notes };
                var response = await _httpClient.PostAsJsonAsync("api/Loyalty/claim", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonDocument>();
                    string msg = "Reward claimed successfully!";
                    if (result != null && result.RootElement.TryGetProperty("message", out var msgElement))
                    {
                        msg = msgElement.GetString() ?? msg;
                    }
                    return (true, msg);
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<JsonDocument>();
                    string msg = "Failed to claim reward.";
                    if (error != null && error.RootElement.TryGetProperty("message", out var msgElement))
                    {
                        msg = msgElement.GetString() ?? msg;
                    }
                    return (false, msg);
                }
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<IEnumerable<LoyaltyRewardDto>> GetActiveRewardsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<IEnumerable<LoyaltyRewardDto>>("api/Loyalty/rewards") ?? Enumerable.Empty<LoyaltyRewardDto>();
            }
            catch { return Enumerable.Empty<LoyaltyRewardDto>(); }
        }

        #region Users
        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<UserDto>>("api/users") ?? Enumerable.Empty<UserDto>();
        }

        public async Task<UserDto?> GetUserAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<UserDto>($"api/users/{id}");
        }

        public async Task<UserDto?> CreateUserAsync(UserCreateRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/users", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<UserDto>() : null;
        }

        public async Task<UserDto?> UpdateUserAsync(Guid id, UserUpdateRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/users/{id}", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<UserDto>() : null;
        }

        public async Task<bool> UpdateUserRoleAsync(Guid id, string role)
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/users/{id}/role", new UserRoleUpdateRequest { Role = role });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/users/{id}");
            return response.IsSuccessStatusCode;
        }
        #endregion

        public async Task<IEnumerable<LoyaltyRedemptionDto>> GetPendingRedemptionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Loyalty/admin/redemptions/pending");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString };
                    return await response.Content.ReadFromJsonAsync<IEnumerable<LoyaltyRedemptionDto>>(options) ?? Enumerable.Empty<LoyaltyRedemptionDto>();
                }
                return Enumerable.Empty<LoyaltyRedemptionDto>();
            }
            catch { return Enumerable.Empty<LoyaltyRedemptionDto>(); }
        }

        public async Task<(bool Success, string Message)> FulfillRedemptionAsync(string id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/Loyalty/admin/redemptions/{id}/fulfill", null);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonDocument>();
                    string msg = "Redemption fulfilled successfully!";
                    if (result != null && result.RootElement.TryGetProperty("message", out var msgElement))
                    {
                        msg = msgElement.GetString() ?? msg;
                    }
                    return (true, msg);
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<JsonDocument>();
                    string msg = "Failed to fulfill redemption.";
                    if (error != null && error.RootElement.TryGetProperty("message", out var msgElement))
                    {
                        msg = msgElement.GetString() ?? msg;
                    }
                    return (false, msg);
                }
            }
            catch (Exception ex) { return (false, $"Error: {ex.Message}"); }
        }
        
        #region Notifications
        public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<NotificationDto>>("api/notifications") ?? Enumerable.Empty<NotificationDto>();
        }

        public async Task<int> GetUnreadNotificationCountAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<int>("api/notifications/unread-count");
            }
            catch { return 0; }
        }

        public async Task<bool> MarkNotificationsReadAsync()
        {
            var response = await _httpClient.PostAsJsonAsync("api/notifications/mark-read", new { });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var response = await _httpClient.PostAsync($"api/notifications/mark-read/{id}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateFcmTokenAsync(string fcmToken)
        {
            var response = await _httpClient.PostAsJsonAsync("api/users/fcm-token", new { fcmToken });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SyncWishlistAsync(List<int> bookIds)
        {
            var response = await _httpClient.PostAsJsonAsync("api/wishlist/sync", bookIds);
            return response.IsSuccessStatusCode;
        }
        #endregion
    }
}

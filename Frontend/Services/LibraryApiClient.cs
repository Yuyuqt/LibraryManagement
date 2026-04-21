using System.Net.Http.Json;
using Frontend.Models.Dtos;

namespace Frontend.Services
{
    public class LibraryApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LibraryApiClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("LibraryBackend");
            _httpContextAccessor = httpContextAccessor;
            
            AddAuthHeaderFromCookie();
        }

        private void AddAuthHeaderFromCookie()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public void SetToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
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
        public async Task<IEnumerable<BookDto>> GetBooksAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<BookDto>>("api/books") ?? Enumerable.Empty<BookDto>();
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

        public async Task<BorrowingDto?> ReturnBookAsync(int id)
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
            try {
                return await _httpClient.GetFromJsonAsync<SubscriptionDto>("api/subscriptions/me");
            } catch { return null; }
        }

        public async Task<SubscriptionDto?> SubscribeAsync(SubscribeRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/subscriptions/subscribe", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<SubscriptionDto>() : null;
        }

        public async Task<SubscriptionDto?> GetUserSubscriptionAsync(int userId)
        {
            try {
                return await _httpClient.GetFromJsonAsync<SubscriptionDto>($"api/subscriptions/user/{userId}");
            } catch { return null; }
        }

        public async Task<SubscriptionDto?> AdminSubscribeAsync(AdminSubscribeRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/subscriptions/admin-subscribe", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<SubscriptionDto>() : null;
        }
        #endregion

        #region Loyalty
        public async Task<LoyaltyAccountDto?> GetMyLoyaltyAccountAsync()
        {
            try {
                return await _httpClient.GetFromJsonAsync<LoyaltyAccountDto>("api/loyalty/my-account");
            } catch { return null; }
        }
        #endregion

        #region Users
        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<UserDto>>("api/users") ?? Enumerable.Empty<UserDto>();
        }

        public async Task<UserDto?> GetUserAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<UserDto>($"api/users/{id}");
        }

        public async Task<UserDto?> CreateUserAsync(UserCreateRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/users", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<UserDto>() : null;
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UserUpdateRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/users/{id}", request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<UserDto>() : null;
        }

        public async Task<bool> UpdateUserRoleAsync(int id, string role)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/users/role/{id}", new UserRoleUpdateRequest { Role = role });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/users/{id}");
            return response.IsSuccessStatusCode;
        }
        #endregion
    }
}

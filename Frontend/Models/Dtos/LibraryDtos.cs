using System.Text.Json.Serialization;

namespace Frontend.Models.Dtos
{
    // Auth DTOs
    public class RegisterRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? StudentId { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }
    }

    // Book DTOs
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<BookCategoryDto> Categories { get; set; } = new();
    }

    public class BookCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class BookCreateRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TotalCopies { get; set; }
        public List<int>? CategoryIds { get; set; }
    }

    public class BookUpdateRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TotalCopies { get; set; }
        public List<int>? CategoryIds { get; set; }
    }

    // Borrowing DTOs
    public class BorrowingDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; } // This is ActualReturnDate in some contexts
        public string Status { get; set; } = string.Empty;
        public decimal FineAmount { get; set; }
        public bool IsFinePaid { get; set; }
    }

    public class BorrowRequest
    {
        public int BookId { get; set; }
    }

    // Category DTOs
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CategoryCreateRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    // Subscription & Membership DTOs
    public class MembershipDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public int MaxBooks { get; set; }
        public int BorrowingDays { get; set; }
        public decimal Price { get; set; }
        public int DurationMonths { get; set; }
        public string? RewardId { get; set; }
    }

    public class SubscriptionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int MembershipId { get; set; }
        public string MembershipType { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiryDate;
    }

    public class SubscribeRequest
    {
        public int MembershipId { get; set; }
    }

    public class AdminSubscribeRequest
    {
        public Guid UserId { get; set; }
        public int MembershipId { get; set; }
    }

    // Loyalty DTOs
    public class LoyaltyAccountDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // Some responses use `accountId` instead of `id`.
        [JsonPropertyName("accountId")]
        public string? AccountId { get; set; }

        [JsonPropertyName("externalUserId")]
        public string ExternalUserId { get; set; } = string.Empty;

        [JsonPropertyName("currentBalance")]
        public double CurrentBalance { get; set; }

        [JsonPropertyName("tier")]
        public string Tier { get; set; } = string.Empty;

        [JsonPropertyName("lifetimePoints")]
        public double LifetimePoints { get; set; }
    }

    public class LoyaltyRewardDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("pointCost")]
        public double PointCost { get; set; }

        [JsonPropertyName("stockQuantity")]
        public int StockQuantity { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }

    public class ClaimRewardRequestDto
    {
        public string RewardId { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    // User DTOs
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? StudentId { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? BanStatus { get; set; }
        public DateTime? SuspensionEndDate { get; set; }
    }

    public class UserCreateRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? StudentId { get; set; }
        public string? Address { get; set; }
    }

    public class UserUpdateRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? StudentId { get; set; }
        public string? Address { get; set; }
    }

    public class UserRoleUpdateRequest
    {
        public string Role { get; set; } = string.Empty;
    }

    // Points History DTOs
    public class PointHistoryEntryDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("accountId")]
        public string AccountId { get; set; } = string.Empty;
        [JsonPropertyName("externalUserId")]
        public string? ExternalUserId { get; set; }
        [JsonPropertyName("pointDelta")]
        public double PointDelta { get; set; }
        [JsonPropertyName("eventKey")]
        public string EventKey { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("referenceId")]
        public string? ReferenceId { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("rewardId")]
        public string? RewardId { get; set; }

        [JsonPropertyName("rewardName")]
        public string? RewardName { get; set; }

        [JsonPropertyName("redemptionStatus")]
        public string? RedemptionStatus { get; set; }

        [JsonPropertyName("redeemedAt")]
        public DateTime? RedeemedAt { get; set; }
    }

    public class UserPointsHistoryDto
    {
        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }
        [JsonPropertyName("userName")]
        public string UserName { get; set; } = string.Empty;
        [JsonPropertyName("userEmail")]
        public string UserEmail { get; set; } = string.Empty;
        [JsonPropertyName("accountId")]
        public string AccountId { get; set; } = string.Empty;
        [JsonPropertyName("currentBalance")]
        public double CurrentBalance { get; set; }
        [JsonPropertyName("tier")]
        public string Tier { get; set; } = string.Empty;
        [JsonPropertyName("history")]
        public IEnumerable<PointHistoryEntryDto> History { get; set; } = Enumerable.Empty<PointHistoryEntryDto>();
        [JsonPropertyName("redemptions")]
        public IEnumerable<LoyaltyRedemptionDto> Redemptions { get; set; } = Enumerable.Empty<LoyaltyRedemptionDto>();
    }
}

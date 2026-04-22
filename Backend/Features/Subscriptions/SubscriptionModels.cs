using System;

namespace Backend.Features.Subscriptions
{
    public class MembershipDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public int MaxBooks { get; set; }
        public int BorrowingDays { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "MMK";
        public int DurationMonths { get; set; }
        public string? RewardId { get; set; }
    }

    public class SubscriptionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int MembershipId { get; set; }
        public string MembershipType { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
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
}

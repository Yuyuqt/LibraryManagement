using System;

namespace Frontend.Models.Dtos
{
    public class LoyaltyRedemptionDto
    {
        public Guid Id { get; set; }
        public string ExternalUserId { get; set; } = string.Empty;
        public string RewardName { get; set; } = string.Empty;
        public string? LoyaltyRewardId { get; set; }
        public int PointsSpent { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
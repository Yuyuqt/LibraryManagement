using System;
using System.Text.Json.Serialization;

namespace Backend.Features.Loyalty
{
    public class LoyaltyRedemptionDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("rewardId")]
        public string RewardId { get; set; } = string.Empty;

        // Fallback for snake_case from external API
        [JsonPropertyName("reward_id")]
        public string? RewardIdSnakeCase { get => RewardId; set => RewardId = string.IsNullOrEmpty(value) ? RewardId : value; }

        [JsonPropertyName("rewardName")]
        public string RewardName { get; set; } = string.Empty;
        
        [JsonPropertyName("reward_name")]
        public string? RewardNameSnakeCase { get => RewardName; set => RewardName = string.IsNullOrEmpty(value) ? RewardName : value; }

        [JsonPropertyName("pointsSpent")]
        public double PointsSpent { get; set; }

        [JsonPropertyName("points_spent")]
        public double? PointsSpentSnakeCase { set => PointsSpent = value ?? PointsSpent; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("externalUserId")]
        public string ExternalUserId { get; set; } = string.Empty;

        [JsonPropertyName("external_user_id")]
        public string? ExternalUserIdSnakeCase { get => ExternalUserId; set => ExternalUserId = string.IsNullOrEmpty(value) ? ExternalUserId : value; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAtSnakeCase { set => CreatedAt = value ?? CreatedAt; }
    }

    public class ClaimRewardRequestDto
    {
        public string RewardId { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}

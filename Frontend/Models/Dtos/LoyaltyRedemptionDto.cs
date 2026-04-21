using System;
using System.Text.Json.Serialization;

namespace Frontend.Models.Dtos
{
    public class LoyaltyRedemptionDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("externalUserId")]
        public string ExternalUserId { get; set; } = string.Empty;

        [JsonPropertyName("rewardName")]
        public string RewardName { get; set; } = string.Empty;

        [JsonPropertyName("rewardId")]
        public string? RewardId { get; set; }

        [JsonPropertyName("pointsSpent")]
        public double PointsSpent { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
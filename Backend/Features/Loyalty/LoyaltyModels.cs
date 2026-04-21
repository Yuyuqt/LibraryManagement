using System;
using System.Text.Json.Serialization;

namespace Backend.Features.Loyalty
{
    public class LoyaltyRedemptionDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("systemId")]
        public string SystemId { get; set; } = string.Empty;

        [JsonPropertyName("externalUserId")]
        public string ExternalUserId { get; set; } = string.Empty;

        [JsonPropertyName("rewardId")]
        public string RewardId { get; set; } = string.Empty;

        [JsonPropertyName("rewardName")]
        public string RewardName { get; set; } = string.Empty;

        [JsonPropertyName("pointCost")]
        public double PointCost { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("redeemedAt")]
        public DateTime RedeemedAt { get; set; }
    }

    public class ClaimRewardRequestDto
    {
        public string RewardId { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}

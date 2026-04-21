using System;
using System.Text.Json.Serialization;

namespace Frontend.Models.Dtos
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
        public string? RewardId { get; set; }

        [JsonPropertyName("rewardName")]
        public string RewardName { get; set; } = string.Empty;

        [JsonPropertyName("pointCost")]
        public double PointCost { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("redeemedAt")]
        public DateTime RedeemedAt { get; set; }
    }
}
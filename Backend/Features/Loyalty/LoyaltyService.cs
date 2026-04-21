using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Backend.Features.Loyalty
{
    public interface ILoyaltyService
    {
        Task<bool> RegisterUserAsync(string externalUserId, string email, string mobile);
        Task<bool> ProcessEventAsync(string externalUserId, string eventKey, double eventValue, string referenceId, string description, string email, string mobile);
        Task<AccountLookupResponse?> GetUserAccountAsync(string externalUserId);
        Task<(bool Success, string Message)> ClaimRewardAsync(string externalUserId, string rewardId, string notes);
        Task<IEnumerable<LoyaltyRedemptionDto>> GetPendingRedemptionsAsync();
        Task<IEnumerable<LoyaltyRedemptionDto>> GetRedemptionsHistoryAsync();
        Task<bool> UpdateRedemptionStatusAsync(string redemptionId, string status);
    }

    public class LoyaltyService : ILoyaltyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LoyaltyService> _logger;
        private const string SystemId = "THS-LMS";

        public LoyaltyService(HttpClient httpClient, ILogger<LoyaltyService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            if (!_httpClient.DefaultRequestHeaders.Contains("x-system-id"))
            {
                _httpClient.DefaultRequestHeaders.Add("x-system-id", SystemId);
            }
        }

        public async Task<bool> RegisterUserAsync(string externalUserId, string email, string mobile)
        {
            try
            {
                var payload = new
                {
                    systemId = SystemId,
                    externalUserId = externalUserId,
                    email = email,
                    mobile = mobile,
                    tier = "Member" // Default
                };

                var response = await _httpClient.PostAsJsonAsync("/api/v1/accounts", payload);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to register user in loyalty system: {error}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user in loyalty system.");
                return false;
            }
        }

        public async Task<bool> ProcessEventAsync(string externalUserId, string eventKey, double eventValue, string referenceId, string description, string email, string mobile)
        {
            try
            {
                var payload = new
                {
                    externalUserId = externalUserId,
                    eventKey = eventKey,
                    eventValue = eventValue,
                    referenceId = referenceId,
                    description = description,
                    email = email,
                    mobile = mobile
                };

                var response = await _httpClient.PostAsJsonAsync("/api/v1/events/process", payload);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to process loyalty event {eventKey} for user {externalUserId}: {error}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing loyalty event {eventKey}.");
                return false;
            }
        }

        public async Task<AccountLookupResponse?> GetUserAccountAsync(string externalUserId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/accounts/lookup/{SystemId}/{externalUserId}");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return await response.Content.ReadFromJsonAsync<AccountLookupResponse>(options);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching loyalty account for user {externalUserId}.");
                return null;
            }
        }

        public async Task<(bool Success, string Message)> ClaimRewardAsync(string externalUserId, string rewardId, string notes)
        {
            try
            {
                var payload = new
                {
                    externalUserId = externalUserId,
                    rewardId = rewardId,
                    notes = notes
                };

                var response = await _httpClient.PostAsJsonAsync("/api/v1/redemption/claim", payload);
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Reward claimed successfully!");
                }
                
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Failed to claim reward {rewardId} for user {externalUserId}: {error}");
                
                try {
                    var errorDoc = JsonDocument.Parse(error);
                    if (errorDoc.RootElement.TryGetProperty("message", out var msgElement)) {
                        return (false, msgElement.GetString() ?? "Failed to claim reward.");
                    }
                } catch { }

                return (false, string.IsNullOrEmpty(error) ? "Failed to claim reward." : error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while claiming reward.");
                return (false, "An error occurred while processing your request.");
            }
        }

        public async Task<IEnumerable<LoyaltyRedemptionDto>> GetUserRedemptionsAsync(string externalUserId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/redemption/history/{SystemId}/{externalUserId}");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return await response.Content.ReadFromJsonAsync<IEnumerable<LoyaltyRedemptionDto>>(options) ?? Enumerable.Empty<LoyaltyRedemptionDto>();
                }
                return Enumerable.Empty<LoyaltyRedemptionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching redemptions for user {externalUserId}.");
                return Enumerable.Empty<LoyaltyRedemptionDto>();
            }
        }

        public async Task<IEnumerable<LoyaltyRedemptionDto>> GetPendingRedemptionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/v1/admin/redemptions/pending");
                if (response.IsSuccessStatusCode)
                {
                    var rawJson = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Raw Pending Redemptions JSON: {rawJson}");
                    
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<IEnumerable<LoyaltyRedemptionDto>>(rawJson, options) ?? Enumerable.Empty<LoyaltyRedemptionDto>();
                }
                return Enumerable.Empty<LoyaltyRedemptionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending redemptions.");
                return Enumerable.Empty<LoyaltyRedemptionDto>();
            }
        }

        public async Task<IEnumerable<LoyaltyRedemptionDto>> GetRedemptionsHistoryAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/v1/admin/redemptions/history");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return await response.Content.ReadFromJsonAsync<IEnumerable<LoyaltyRedemptionDto>>(options) ?? Enumerable.Empty<LoyaltyRedemptionDto>();
                }
                return Enumerable.Empty<LoyaltyRedemptionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching redemptions history.");
                return Enumerable.Empty<LoyaltyRedemptionDto>();
            }
        }

        public async Task<bool> UpdateRedemptionStatusAsync(string redemptionId, string status)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/api/v1/admin/redemptions/{redemptionId}/status", new { status });
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating redemption status to {status} for {redemptionId}.");
                return false;
            }
        }
    }

    public class AccountLookupResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("externalUserId")]
        public string ExternalUserId { get; set; } = string.Empty;

        [JsonPropertyName("currentBalance")]
        public double CurrentBalance { get; set; }

        [JsonPropertyName("tier")]
        public string Tier { get; set; } = string.Empty;

        [JsonPropertyName("lifetimePoints")]
        public double LifetimePoints { get; set; }
    }
}

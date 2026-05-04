using LibraryManagement.Shared.Models;
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
        Task<bool> ProcessEventAsync(string externalUserId, string eventKey, double eventValue, string? referenceId = null, string? description = null, string? email = null, string? mobile = null);
        Task<LoyaltyAccountDto?> GetUserAccountAsync(string externalUserId);
        Task<(bool Success, string Message)> ClaimRewardAsync(string externalUserId, string rewardId, string notes);
        Task<IEnumerable<LoyaltyRedemptionDto>> GetPendingRedemptionsAsync();
        Task<IEnumerable<LoyaltyRedemptionDto>> GetUserRedemptionsAsync(string externalUserId);
        Task<IEnumerable<LoyaltyRedemptionDto>> GetRedemptionsHistoryAsync();
        Task<bool> UpdateRedemptionStatusAsync(string redemptionId, string status);
        Task<IEnumerable<PointHistoryEntryDto>> GetPointsHistoryAsync(string accountId);
        Task<IEnumerable<LoyaltyRewardDto>> GetActiveRewardsAsync();
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
            _httpClient.DefaultRequestHeaders.Remove("x-system-id");
            _httpClient.DefaultRequestHeaders.Add("x-system-id", SystemId);
        }

        private class LoyaltyResponseWrapper<T>
        {
            [JsonPropertyName("items")]
            public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        }

        public async Task<bool> RegisterUserAsync(string externalUserId, string email, string mobile)
        {
            try
            {
                var payload = new
                {
                    externalUserId = externalUserId,
                    email = email,
                    mobile = mobile,
                    systemId = SystemId
                };

                var response = await _httpClient.PostAsJsonAsync("/api/v1/accounts", payload);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user in loyalty system.");
                return false;
            }
        }

        public async Task<bool> ProcessEventAsync(string externalUserId, string eventKey, double eventValue, string? referenceId = null, string? description = null, string? email = null, string? mobile = null)
        {
            try
            {
                var payload = new
                {
                    systemId = SystemId,
                    externalUserId = externalUserId,
                    eventKey = eventKey,
                    eventValue = eventValue,
                    referenceId = referenceId,
                    description = description,
                    email = email,
                    mobile = mobile
                };

                var response = await _httpClient.PostAsJsonAsync($"/api/v1/events/process?systemId={SystemId}", payload);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing loyalty event {eventKey}.");
                return false;
            }
        }

        public async Task<LoyaltyAccountDto?> GetUserAccountAsync(string externalUserId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/accounts/lookup/{externalUserId}?systemId={SystemId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString };
                    var account = await response.Content.ReadFromJsonAsync<LoyaltyAccountDto>(options);
                    
                    if (account != null && string.IsNullOrWhiteSpace(account.Id) && !string.IsNullOrWhiteSpace(account.AccountId))
                    {
                        account.Id = account.AccountId;
                    }
                    return account;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error looking up loyalty account for {externalUserId}.");
                return null;
            }
        }

        public async Task<(bool Success, string Message)> ClaimRewardAsync(string externalUserId, string rewardId, string notes)
        {
            try
            {
                var payload = new
                {
                    systemId = SystemId,
                    externalUserId = externalUserId,
                    rewardId = rewardId,
                    notes = notes
                };

                var response = await _httpClient.PostAsJsonAsync($"/api/v1/redemptions/claim?systemId={SystemId}", payload);
                if (response.IsSuccessStatusCode) return (true, "Reward claimed successfully!");

                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while claiming reward.");
                return (false, "An error occurred.");
            }
        }

        public async Task<IEnumerable<LoyaltyRedemptionDto>> GetUserRedemptionsAsync(string externalUserId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/redemptions/history/{externalUserId}?systemId={SystemId}");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString };
                    var wrapper = await response.Content.ReadFromJsonAsync<LoyaltyResponseWrapper<LoyaltyRedemptionDto>>(options);
                    return wrapper?.Items ?? Enumerable.Empty<LoyaltyRedemptionDto>();
                }
                return Enumerable.Empty<LoyaltyRedemptionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching user redemptions for {externalUserId}.");
                return Enumerable.Empty<LoyaltyRedemptionDto>();
            }
        }

        public async Task<IEnumerable<LoyaltyRedemptionDto>> GetPendingRedemptionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/admin/redemptions/pending?systemId={SystemId}");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString };
                    var wrapper = await response.Content.ReadFromJsonAsync<LoyaltyResponseWrapper<LoyaltyRedemptionDto>>(options);
                    return wrapper?.Items ?? Enumerable.Empty<LoyaltyRedemptionDto>();
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to fetch pending redemptions. Status: {response.StatusCode}, Content: {content}");
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
                var response = await _httpClient.GetAsync($"/api/v1/admin/redemptions/history?systemId={SystemId}");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString };
                    var wrapper = await response.Content.ReadFromJsonAsync<LoyaltyResponseWrapper<LoyaltyRedemptionDto>>(options);
                    return wrapper?.Items ?? Enumerable.Empty<LoyaltyRedemptionDto>();
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to fetch redemption history. Status: {response.StatusCode}, Content: {content}");
                }
                return Enumerable.Empty<LoyaltyRedemptionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching redemption history.");
                return Enumerable.Empty<LoyaltyRedemptionDto>();
            }
        }

        public async Task<bool> UpdateRedemptionStatusAsync(string redemptionId, string status)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/api/v1/admin/redemptions/{redemptionId}/status?systemId={SystemId}", new { systemId = SystemId, status });
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to update redemption status. Status: {response.StatusCode}, Content: {content}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating redemption status for {redemptionId}.");
                return false;
            }
        }

        public async Task<IEnumerable<PointHistoryEntryDto>> GetPointsHistoryAsync(string accountId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/accounts/{accountId}/history?systemId={SystemId}");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString };
                    return await response.Content.ReadFromJsonAsync<IEnumerable<PointHistoryEntryDto>>(options) ?? Enumerable.Empty<PointHistoryEntryDto>();
                }
                return Enumerable.Empty<PointHistoryEntryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching points history for {accountId}.");
                return Enumerable.Empty<PointHistoryEntryDto>();
            }
        }

        public async Task<IEnumerable<LoyaltyRewardDto>> GetActiveRewardsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/rewards/active?systemId={SystemId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString };
                    return await response.Content.ReadFromJsonAsync<IEnumerable<LoyaltyRewardDto>>(options) ?? Enumerable.Empty<LoyaltyRewardDto>();
                }
                return Enumerable.Empty<LoyaltyRewardDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching active rewards.");
                return Enumerable.Empty<LoyaltyRewardDto>();
            }
        }
    }
}

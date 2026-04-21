using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Backend.Features.Loyalty
{
    public interface ILoyaltyService
    {
        Task<bool> RegisterUserAsync(string externalUserId, string email, string mobile);
        Task<bool> ProcessEventAsync(string externalUserId, string eventKey, double eventValue, string referenceId, string description, string email, string mobile);
        Task<AccountLookupResponse?> GetUserAccountAsync(string externalUserId);
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
            _httpClient.DefaultRequestHeaders.Add("x-system-id", SystemId);
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

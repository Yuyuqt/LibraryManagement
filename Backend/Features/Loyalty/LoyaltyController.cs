using Backend.Features.Subscriptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace Backend.Features.Loyalty
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoyaltyController : ControllerBase
    {
        private readonly ILoyaltyService _loyaltyService;
        private readonly ISubscriptionService _subscriptionService;

        public LoyaltyController(ILoyaltyService loyaltyService, ISubscriptionService subscriptionService)
        {
            _loyaltyService = loyaltyService;
            _subscriptionService = subscriptionService;
        }

        [HttpGet("my-account")]
        public async Task<IActionResult> GetMyAccount()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized();
            }

            var account = await _loyaltyService.GetUserAccountAsync(userIdStr);
            if (account == null)
            {
                return NotFound("Loyalty account not found.");
            }

            return Ok(account);
        }

        [HttpGet("my-redemptions")]
        public async Task<IActionResult> GetMyRedemptions()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized();
            }

            var redemptions = await _loyaltyService.GetRedemptionsHistoryAsync();
            return Ok(redemptions);
        }

        [HttpPost("claim")]
        public async Task<IActionResult> ClaimReward([FromBody] ClaimRewardRequestDto request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized();
            }

            var (success, message) = await _loyaltyService.ClaimRewardAsync(userIdStr, request.RewardId, request.Notes ?? "Redeemed via Library Web Application");

            if (success)
            {
                return Ok(new { message });
            }

            return BadRequest(new { message });
        }

        [HttpGet("admin/redemptions/pending")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> GetPendingRedemptions()
        {
            var redemptions = await _loyaltyService.GetPendingRedemptionsAsync();
            return Ok(redemptions);
        }

        [HttpPost("admin/redemptions/{id}/fulfill")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> FulfillRedemption(string id)
        {
            // 1. Get all pending redemptions to find the details of the one we want
            var pending = await _loyaltyService.GetPendingRedemptionsAsync();
            var redemption = pending.FirstOrDefault(r => r.Id == id);

            if (redemption == null)
            {
                return NotFound("Redemption record not found or already processed.");
            }

            // 2. Call the external loyalty system to mark as fulfilled
            var success = await _loyaltyService.UpdateRedemptionStatusAsync(id, "Fulfilled");
            if (!success)
            {
                return BadRequest("Failed to update status in the loyalty system.");
            }

            // 3. side effect: Grant membership in the library system
            bool membershipGranted = false;
            if (int.TryParse(redemption.ExternalUserId?.Trim(), out int userId))
            {
                System.Diagnostics.Debug.WriteLine($"Fulfilling redemption {id} for user {userId}. RewardId: '{redemption.RewardId}'");
                membershipGranted = await _subscriptionService.HandleLoyaltyRedemptionAsync(userId, redemption.RewardId, redemption.Id);
            }

            if (!membershipGranted)
            {
                var errorMsg = $"Redemption fulfilled in loyalty system, but failed to grant library membership. The Reward ID '{redemption.RewardId}' (Redemption ID: {id}) may not be correctly mapped in the library system, or the user already has researchers this membership.";
                System.Diagnostics.Debug.WriteLine(errorMsg);
                return BadRequest(new { message = errorMsg });
            }

            return Ok(new { message = $"Redemption fulfilled and {redemption.RewardName} membership granted successfully." });
        }
    }
}

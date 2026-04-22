using Backend.Features.Subscriptions;
using DbConnect.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Features.Loyalty
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoyaltyController : ControllerBase
    {
        private readonly ILoyaltyService _loyaltyService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly AppDbContext _context;

        public LoyaltyController(ILoyaltyService loyaltyService, ISubscriptionService subscriptionService, AppDbContext context)
        {
            _loyaltyService = loyaltyService;
            _subscriptionService = subscriptionService;
            _context = context;
        }

        // ── Member endpoints ──────────────────────────────────────────────────

        [HttpGet("my-account")]
        public async Task<IActionResult> GetMyAccount()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var account = await _loyaltyService.GetUserAccountAsync(userIdStr);
            if (account == null) return NotFound("Loyalty account not found.");
            return Ok(account);
        }

        [HttpGet("my-redemptions")]
        public async Task<IActionResult> GetMyRedemptions()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            // Filter to this user's redemptions only
            var redemptions = await _loyaltyService.GetUserRedemptionsAsync(userIdStr);
            return Ok(redemptions);
        }

        [HttpGet("my-pending-redemptions")]
        public async Task<IActionResult> GetMyPendingRedemptions()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var redemptions = await _loyaltyService.GetUserRedemptionsAsync(userIdStr);
            var pending = redemptions.Where(r => r.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));
            return Ok(pending);
        }

        [HttpGet("my-points-history")]
        public async Task<IActionResult> GetMyPointsHistory()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var account = await _loyaltyService.GetUserAccountAsync(userIdStr);
            if (account == null) return NotFound("Loyalty account not found.");

            var history = await _loyaltyService.GetPointsHistoryAsync(account.Id);
            return Ok(history);
        }

        [HttpPost("claim")]
        public async Task<IActionResult> ClaimReward([FromBody] ClaimRewardRequestDto request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var (success, message) = await _loyaltyService.ClaimRewardAsync(userIdStr, request.RewardId, request.Notes ?? "Redeemed via Library Web Application");
            if (success) return Ok(new { message });
            return BadRequest(new { message });
        }

        // ── Librarian endpoints ───────────────────────────────────────────────

        [HttpGet("admin/redemptions/pending")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> GetPendingRedemptions()
        {
            var redemptions = await _loyaltyService.GetPendingRedemptionsAsync();
            return Ok(redemptions);
        }

        [HttpGet("admin/all-points-history")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> GetAllMembersPointsHistory()
        {
            // Get all users from DB
            var users = await _context.Users
                .Select(u => new { u.Id, u.FullName, u.Email })
                .ToListAsync();

            var results = new List<object>();

            foreach (var user in users)
            {
                try
                {
                    var account = await _loyaltyService.GetUserAccountAsync(user.Id.ToString());
                    if (account == null) continue;

                    var accountId = !string.IsNullOrWhiteSpace(account.Id) ? account.Id : (account.AccountId ?? string.Empty);
                    if (string.IsNullOrWhiteSpace(accountId)) continue;

                    var history = await _loyaltyService.GetPointsHistoryAsync(accountId);
                    var redemptions = await _loyaltyService.GetUserRedemptionsAsync(user.Id.ToString());
                    results.Add(new
                    {
                        userId = user.Id,
                        userName = user.FullName,
                        userEmail = user.Email,
                        accountId = accountId,
                        currentBalance = account.CurrentBalance,
                        tier = account.Tier,
                        history = history,
                        redemptions = redemptions
                    });
                }
                catch { /* Skip users not in loyalty system */ }
            }

            return Ok(results);
        }

        [HttpPost("admin/redemptions/{id}/fulfill")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> FulfillRedemption(string id)
        {
            var pending = await _loyaltyService.GetPendingRedemptionsAsync();
            var redemption = pending.FirstOrDefault(r => r.Id == id);

            if (redemption == null)
                return NotFound("Redemption record not found or already processed.");

            var success = await _loyaltyService.UpdateRedemptionStatusAsync(id, "Fulfilled");
            if (!success)
                return BadRequest("Failed to update status in the loyalty system.");

            bool membershipGranted = false;
            if (Guid.TryParse(redemption.ExternalUserId?.Trim(), out Guid userId))
            {
                System.Diagnostics.Debug.WriteLine($"Fulfilling redemption {id} for user {userId}. RewardId: '{redemption.RewardId}', RewardName: '{redemption.RewardName}'");
                membershipGranted = await _subscriptionService.HandleLoyaltyRedemptionAsync(userId, redemption.RewardId, redemption.RewardName, redemption.Id);
            }

            if (!membershipGranted)
            {
                var errorMsg = $"Redemption fulfilled in loyalty system, but failed to grant library membership. RewardName='{redemption.RewardName}', RewardId='{redemption.RewardId}' (Redemption ID: {id}).";
                return BadRequest(new { message = errorMsg });
            }

            return Ok(new { message = $"Redemption fulfilled and {redemption.RewardName} membership granted successfully." });
        }
    }

}

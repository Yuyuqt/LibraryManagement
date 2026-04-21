using Microsoft.AspNetCore.Mvc;
using Frontend.Services;
using Frontend.Models.Dtos;
using Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Frontend.Controllers
{
    [Authorize(Roles = "Member")]
    public class MembershipController : Controller
    {
        private readonly LibraryApiClient _apiClient;

        public MembershipController(LibraryApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var subscription = await _apiClient.GetMySubscriptionAsync();
            var memberships = await _apiClient.GetMembershipsAsync();
            var redemptions = await _apiClient.GetMyRedemptionsAsync();
            var loyaltyAccount = await _apiClient.GetMyLoyaltyAccountAsync();
            var activeRewards = await _apiClient.GetActiveRewardsAsync();

            // Filter redemptions to only show memberships that are Pending (Queued)
            // We use LoyaltyRewardId to identify which redemptions are memberships
            var membershipRewardIds = memberships
                .Where(m => !string.IsNullOrEmpty(m.RewardId))
                .Select(m => m.RewardId)
                .ToList();
            
            var queuedMemberships = redemptions
                .Where(r => (r.Status == "Pending" || r.Status == "PENDING" || r.Status == "Fulfilled" || r.Status == "FULFILLED") && membershipRewardIds.Contains(r.RewardId))
                .ToList();

            var viewModel = new MembershipViewModel
            {
                CurrentSubscription = subscription,
                AvailableMemberships = memberships.ToList(),
                QueuedMemberships = queuedMemberships,
                LoyaltyAccount = loyaltyAccount,
                RewardPointCosts = activeRewards.ToDictionary(r => r.Id, r => r.PointCost)
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Redeem(string rewardId)
        {
            var (success, message) = await _apiClient.ClaimRewardAsync(rewardId, "Redeemed via Library Membership Page");
            
            if (success)
            {
                return Json(new { success = true, message });
            }
            
            return Json(new { success = false, message });
        }
    }
}

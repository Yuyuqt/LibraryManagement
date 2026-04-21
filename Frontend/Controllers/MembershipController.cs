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
            var allSubscriptions = await _apiClient.GetMyAllSubscriptionsAsync();
            var memberships = await _apiClient.GetMembershipsAsync();
            var loyaltyAccount = await _apiClient.GetMyLoyaltyAccountAsync();
            var activeRewards = await _apiClient.GetActiveRewardsAsync();

            var now = DateTime.UtcNow;

            // The currently active subscription is the one whose time range includes now
            var currentSubscription = allSubscriptions
                .Where(s => s.StartDate <= now && s.ExpiryDate > now)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefault();

            // Queued = subscriptions that haven't started yet (StartDate in the future)
            var queuedMemberships = allSubscriptions
                .Where(s => s.StartDate > now)
                .OrderBy(s => s.StartDate)
                .ToList();

            var viewModel = new MembershipViewModel
            {
                CurrentSubscription = currentSubscription,
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

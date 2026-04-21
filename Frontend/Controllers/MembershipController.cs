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

            // Filter redemptions to only show memberships that are Pending (Queued)
            // We use LoyaltyRewardId to identify which redemptions are memberships
            var membershipRewardIds = memberships
                .Where(m => !string.IsNullOrEmpty(m.LoyaltyRewardId))
                .Select(m => m.LoyaltyRewardId)
                .ToList();
            
            var queuedMemberships = redemptions
                .Where(r => (r.Status == "Pending" || r.Status == "PENDING") && membershipRewardIds.Contains(r.LoyaltyRewardId))
                .ToList();

            var viewModel = new MembershipViewModel
            {
                CurrentSubscription = subscription,
                AvailableMemberships = memberships.ToList(),
                QueuedMemberships = queuedMemberships,
                LoyaltyAccount = loyaltyAccount
            };

            return View(viewModel);
        }
    }
}

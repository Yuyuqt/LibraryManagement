using Frontend.Models;
using Frontend.Models.Dtos;
using Frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    [Authorize]
    public class RewardsController : Controller
    {
        private readonly LibraryApiClient _apiClient;

        public RewardsController(LibraryApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // Member view: available rewards + pending redemptions + points history
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var rewards = await _apiClient.GetActiveRewardsAsync();
            var account = await _apiClient.GetMyLoyaltyAccountAsync();
            var pending = await _apiClient.GetMyPendingRedemptionsAsync();
            var redemptions = await _apiClient.GetMyRedemptionsAsync();
            if (account != null && string.IsNullOrWhiteSpace(account.Id) && !string.IsNullOrWhiteSpace(account.AccountId))
                account.Id = account.AccountId;

            var history = account != null && !string.IsNullOrWhiteSpace(account.Id)
                ? await _apiClient.GetPointsHistoryAsync(account.Id)
                : Enumerable.Empty<PointHistoryEntryDto>();

            var vm = new RewardsViewModel
            {
                Rewards = rewards,
                Account = account,
                PendingRedemptions = pending,
                RedemptionsHistory = redemptions,
                PointsHistory = history
            };

            return View(vm);
        }

        // Librarian view: all pending + all members' point history
        [HttpGet]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Manage()
        {
            var pending = await _apiClient.GetPendingRedemptionsAsync();
            var allHistory = (await _apiClient.GetAllMembersPointsHistoryAsync())
                .Select(m =>
                {
                    m.History = m.History?.ToList() ?? new List<PointHistoryEntryDto>();
                    m.Redemptions = m.Redemptions?.ToList() ?? new List<LoyaltyRedemptionDto>();
                    return m;
                })
                .ToList();

            var vm = new LibrarianRewardsViewModel
            {
                PendingRedemptions = pending,
                AllMembersHistory = allHistory
            };

            return View(vm);
        }

        // POST: fulfill a redemption (librarian)
        [HttpPost]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Fulfill(string id)
        {
            var (success, message) = await _apiClient.FulfillRedemptionAsync(id);
            if (success)
                TempData["Success"] = message;
            else
                TempData["Error"] = message;

            return RedirectToAction(nameof(Manage));
        }

        // POST: redeem a reward (member)
        [HttpPost]
        public async Task<IActionResult> Redeem(string rewardId)
        {
            var (success, message) = await _apiClient.ClaimRewardAsync(rewardId, "Redeemed via Library Rewards Page");
            return Json(new { success, message });
        }
    }
}

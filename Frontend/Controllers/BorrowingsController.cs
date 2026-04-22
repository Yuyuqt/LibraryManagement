using Frontend.Models;
using Frontend.Models.Dtos;
using Frontend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class BorrowingsController : Controller
    {
        private readonly LibraryApiClient _apiClient;

        public BorrowingsController(LibraryApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var borrowings = await _apiClient.GetMyBorrowingsAsync();
            return View(borrowings);
        }

        [HttpPost]
        public async Task<IActionResult> RequestReturn(Guid id)
        {
            var success = await _apiClient.RequestReturnAsync(id);
            if (success)
            {
                TempData["Success"] = "Return request sent! Awaiting librarian approval.";
            }
            else
            {
                TempData["Error"] = "Failed to request return.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Rewards()
        {
            var rewards = await _apiClient.GetActiveRewardsAsync();
            var account = await _apiClient.GetMyLoyaltyAccountAsync();
            
            var viewModel = new RewardsViewModel
            {
                Rewards = rewards,
                Account = account
            };
            
            return View(viewModel);
        }

        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Manage()
        {
            var allBorrowings = await _apiClient.GetAllBorrowingsAsync();
            return View(allBorrowings);
        }

        [HttpPost]
        public async Task<IActionResult> Borrow(int bookId)
        {
            var request = new BorrowRequest { BookId = bookId };
            var result = await _apiClient.BorrowBookAsync(request);

            if (result != null)
            {
                TempData["Success"] = "Book borrowed successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to borrow book. Check your subscription or book availability.";
            return RedirectToAction("Details", "Books", new { id = bookId });
        }

        [HttpPost]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Return(Guid id, bool fromManage = false)
        {
            var result = await _apiClient.ReturnBookAsync(id);
            if (result != null)
            {
                TempData["Success"] = "Book returned successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to process return.";
            }
            
            if (fromManage)
                return RedirectToAction(nameof(Manage));
            
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Redeem(string rewardId)
        {
            var (success, message) = await _apiClient.ClaimRewardAsync(rewardId, "Redeemed via Library Rewards Page");
            
            if (success)
            {
                return Json(new { success = true, message });
            }
            
            return Json(new { success = false, message });
        }
    }
}

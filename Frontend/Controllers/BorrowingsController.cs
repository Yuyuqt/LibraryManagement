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
        public async Task<IActionResult> Return(int id, bool fromManage = false)
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
    }
}

using Microsoft.AspNetCore.Mvc;
using Frontend.Services;
using Frontend.Models.Dtos;

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
            if (User.IsInRole("Admin"))
            {
                var allBorrowings = await _apiClient.GetAllBorrowingsAsync();
                return View(allBorrowings);
            }
            else
            {
                var borrowings = await _apiClient.GetMyBorrowingsAsync();
                return View(borrowings);
            }
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
        public async Task<IActionResult> Return(int id)
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
            return RedirectToAction(nameof(Index));
        }
    }
}

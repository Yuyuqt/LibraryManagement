using Microsoft.AspNetCore.Mvc;
using Frontend.Services;
using Frontend.Models.Dtos;

namespace Frontend.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryApiClient _apiClient;

        public BooksController(LibraryApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var books = await _apiClient.GetBooksAsync();
            return View(books);
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _apiClient.GetBookAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _apiClient.GetCategoriesAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _apiClient.GetCategoriesAsync();
                return View(request);
            }

            var result = await _apiClient.CreateBookAsync(request);
            if (result != null)
            {
                TempData["Success"] = "Book added successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to create book.");
            ViewBag.Categories = await _apiClient.GetCategoriesAsync();
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _apiClient.GetBookAsync(id);
            if (book == null) return NotFound();

            ViewBag.Categories = await _apiClient.GetCategoriesAsync();
            return View(new BookUpdateRequest 
            { 
                Title = book.Title, 
                Author = book.Author, 
                Description = book.Description,
                TotalCopies = book.TotalCopies
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, BookUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _apiClient.GetCategoriesAsync();
                return View(request);
            }

            var result = await _apiClient.UpdateBookAsync(id, request);
            if (result != null)
            {
                TempData["Success"] = "Book updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to update book.");
            ViewBag.Categories = await _apiClient.GetCategoriesAsync();
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _apiClient.DeleteBookAsync(id);
            if (success)
            {
                TempData["Success"] = "Book removed from catalog.";
            }
            else
            {
                TempData["Error"] = "Failed to delete book.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

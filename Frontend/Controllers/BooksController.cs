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

        public async Task<IActionResult> Index(int page = 1, int? categoryId = null)
        {
            const int pageSize = 8;
            var allBooks = await _apiClient.GetBooksAsync(categoryId) ?? Enumerable.Empty<BookDto>();
            var totalCount = allBooks.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var pagedResult = new Frontend.Models.PagedResult<BookDto>
            {
                Items = allBooks.Skip((page - 1) * pageSize).Take(pageSize),
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };

            ViewBag.Categories = await _apiClient.GetCategoriesWithBooksAsync();
            ViewBag.SelectedCategoryId = categoryId;

            return View(pagedResult);
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
                TotalCopies = book.TotalCopies,
                CategoryIds = book.Categories.Select(c => c.Id).ToList()
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

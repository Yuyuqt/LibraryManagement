using Microsoft.AspNetCore.Mvc;
using Frontend.Services;
using System.Diagnostics;
using Frontend.Models;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly LibraryApiClient _apiClient;
        private readonly ILogger<HomeController> _logger;

        public HomeController(LibraryApiClient apiClient, ILogger<HomeController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try 
            {
                var books = await _apiClient.GetBooksAsync();
                var borrowings = await _apiClient.GetMyBorrowingsAsync();
                var members = await _apiClient.GetUsersAsync();
                
                ViewBag.TotalBooks = books.Count();
                ViewBag.ActiveLoans = borrowings.Count(b => !b.ReturnDate.HasValue);
                ViewBag.OverdueLoans = borrowings.Count(b => !b.ReturnDate.HasValue && b.DueDate < DateTime.UtcNow);
                ViewBag.TotalMembers = members.Count();
                
                ViewBag.RecentBooks = books.Take(6);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dashboard data");
                // Set default values if API fails
                ViewBag.TotalBooks = 0;
                ViewBag.ActiveLoans = 0;
                ViewBag.OverdueLoans = 0;
                ViewBag.TotalMembers = 0;
                ViewBag.RecentBooks = Enumerable.Empty<Frontend.Models.Dtos.BookDto>();
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

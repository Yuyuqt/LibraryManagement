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
            // Set defaults to avoid nulls
            ViewBag.TotalBooks = 0;
            ViewBag.ActiveLoans = 0;
            ViewBag.OverdueLoans = 0;
            ViewBag.TotalMembers = 0;
            ViewBag.LoyaltyPoints = 0;
            ViewBag.LoyaltyStatus = "Not Authenticated";
            ViewBag.RecentBooks = Enumerable.Empty<Frontend.Models.Dtos.BookDto>();

            try 
            {
                // 1. Basic Stats (Public)
                var books = await _apiClient.GetBooksAsync();
                ViewBag.TotalBooks = books.Count();
                ViewBag.RecentBooks = books.Take(6);

                // 2. Personal/Authenticated Stats
                if (User.Identity?.IsAuthenticated == true)
                {
                    // Fetch borrowings
                    try {
                        var borrowings = await _apiClient.GetMyBorrowingsAsync();
                        ViewBag.ActiveLoans = borrowings.Count(b => !b.ReturnDate.HasValue);
                        ViewBag.OverdueLoans = borrowings.Count(b => !b.ReturnDate.HasValue && b.DueDate < DateTime.UtcNow);
                    } catch { /* Ignore personal stats failure */ }

                    // Fetch loyalty
                    try {
                        var loyaltyAccount = await _apiClient.GetMyLoyaltyAccountAsync();
                        if (loyaltyAccount != null)
                        {
                            ViewBag.LoyaltyPoints = loyaltyAccount.CurrentBalance;
                            ViewBag.LoyaltyTier = loyaltyAccount.Tier;
                            ViewBag.LoyaltyStatus = "Connected";
                        }
                        else {
                            ViewBag.LoyaltyStatus = "Account Not Linked";
                        }
                    } catch { 
                        ViewBag.LoyaltyStatus = "Loyalty System Unavailable";
                    }

                    // 3. Librarian Only Stats
                    if (User.IsInRole("Librarian"))
                    {
                        try {
                            var members = await _apiClient.GetUsersAsync();
                            ViewBag.TotalMembers = members.Count();
                        } catch { /* Ignore member list failure for non-librarians */ }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical failure on dashboard. Some data might be missing.");
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

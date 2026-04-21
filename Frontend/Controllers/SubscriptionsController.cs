using Microsoft.AspNetCore.Mvc;
using Frontend.Services;
using Frontend.Models.Dtos;
using Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;

namespace Frontend.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SubscriptionsController : Controller
    {
        private readonly LibraryApiClient _apiClient;

        public SubscriptionsController(LibraryApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var memberships = await _apiClient.GetMembershipsAsync();
            var activeSubscriptions = await _apiClient.GetAllSubscriptionsAsync();

            var viewModel = new SubscriptionsViewModel
            {
                AvailableMemberships = memberships?.ToList() ?? new List<MembershipDto>(),
                ActiveSubscriptions = activeSubscriptions?.ToList() ?? new List<SubscriptionDto>()
            };

            return View(viewModel);
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using Frontend.Services;
using Frontend.Models.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace Frontend.Controllers
{
    [Authorize(Roles = "Librarian")]
    public class MembersController : Controller
    {
        private readonly LibraryApiClient _apiClient;

        public MembersController(LibraryApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;
            var allUsers = await _apiClient.GetUsersAsync() ?? Enumerable.Empty<Frontend.Models.Dtos.UserDto>();
            var totalCount = allUsers.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var pagedResult = new Frontend.Models.PagedResult<Frontend.Models.Dtos.UserDto>
            {
                Items = allUsers.Skip((page - 1) * pageSize).Take(pageSize),
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };

            return View(pagedResult);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var user = await _apiClient.GetUserAsync(id);
            if (user == null) return NotFound();

            var memberships = await _apiClient.GetMembershipsAsync();
            var subscription = await _apiClient.GetUserSubscriptionAsync(id);

            var viewModel = new Frontend.Models.MemberDetailsViewModel
            {
                User = user,
                AvailableMemberships = memberships?.ToList() ?? new List<MembershipDto>(),
                CurrentSubscription = subscription
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AssignSubscription(Guid id, int membershipId)
        {
            var request = new AdminSubscribeRequest { UserId = id, MembershipId = membershipId };
            var result = await _apiClient.AdminSubscribeAsync(request);
            if (result != null)
            {
                TempData["Success"] = "Subscription assigned successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to assign subscription.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCreateRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var result = await _apiClient.CreateUserAsync(request);
            if (result != null)
            {
                TempData["Success"] = "Member created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to create member. Email might already exist.");
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var user = await _apiClient.GetUserAsync(id);
            if (user == null) return NotFound();

            return View(new UserUpdateRequest 
            { 
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                StudentId = user.StudentId,
                Address = user.Address
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, UserUpdateRequest request)
        {
            if (!ModelState.IsValid) return View(request);

            var result = await _apiClient.UpdateUserAsync(id, request);
            if (result != null)
            {
                TempData["Success"] = "Member updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to update member.");
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _apiClient.DeleteUserAsync(id);
            if (success)
            {
                TempData["Success"] = "Member removed.";
            }
            else
            {
                TempData["Error"] = "Failed to delete member.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(Guid id, string role)
        {
            var success = await _apiClient.UpdateUserRoleAsync(id, role);
            if (success)
            {
                TempData["Success"] = $"Role updated to {role}.";
            }
            else
            {
                TempData["Error"] = "Failed to update role.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

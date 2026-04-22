using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Loyalty
{
    [ApiController]
    [Route("api/v1/accounts")]
    [Authorize]
    public class LoyaltyAccountsController : ControllerBase
    {
        private readonly ILoyaltyService _loyaltyService;

        public LoyaltyAccountsController(ILoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        [HttpGet("{accountId}/history")]
        public async Task<IActionResult> GetAccountHistory(string accountId)
        {
            // Librarians may view any account history; members may only view their own.
            if (!User.IsInRole("Librarian"))
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

                var myAccount = await _loyaltyService.GetUserAccountAsync(userIdStr);
                if (myAccount == null) return NotFound("Loyalty account not found.");
                if (!string.Equals(myAccount.Id, accountId, StringComparison.OrdinalIgnoreCase))
                    return Forbid();
            }

            var history = await _loyaltyService.GetPointsHistoryAsync(accountId);
            return Ok(history);
        }
    }
}


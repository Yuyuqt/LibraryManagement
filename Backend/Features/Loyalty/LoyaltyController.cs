using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Backend.Features.Loyalty
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoyaltyController : ControllerBase
    {
        private readonly ILoyaltyService _loyaltyService;

        public LoyaltyController(ILoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        [HttpGet("my-account")]
        public async Task<IActionResult> GetMyAccount()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized();
            }

            var account = await _loyaltyService.GetUserAccountAsync(userIdStr);
            if (account == null)
            {
                return NotFound("Loyalty account not found.");
            }

            return Ok(new 
            { 
                currentBalance = account.CurrentBalance, 
                tier = account.Tier 
            });
        }
    }
}

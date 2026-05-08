using LibraryManagement.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Features.Wallet
{
    [ApiController]
    [Route("api/wallet")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet("balance")]
        public async Task<ActionResult<decimal>> GetBalance()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var balance = await _walletService.GetBalanceAsync(Guid.Parse(userIdStr));
            return Ok(balance);
        }

        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<WalletTransactionDto>>> GetHistory()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var history = await _walletService.GetHistoryAsync(Guid.Parse(userIdStr));
            return Ok(history);
        }

        [HttpPost("topup")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> TopUp([FromBody] TopUpRequest request)

        {
            try 
            {
                var librarianIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(librarianIdStr)) return Unauthorized();

                var success = await _walletService.TopUpAsync(
                    request.UserId, 
                    request.Amount, 
                    Guid.Parse(librarianIdStr), 
                    request.Description);

                if (!success) return BadRequest(new { message = "User not found or database error." });

                return Ok(new { message = "Wallet topped up successfully." });
            }
            catch (Exception ex)
            {
                // Return the actual error message to help debug (e.g., missing table error)
                return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }
}

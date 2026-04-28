using LibraryManagement.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Features.Wishlist
{
    [ApiController]
    [Route("api/wishlist")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [HttpPost("sync")]
        public async Task<IActionResult> SyncWishlist([FromBody] List<int> bookIds)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
            {
                return Unauthorized();
            }

            var success = await _wishlistService.SyncWishlistAsync(userId, bookIds);
            if (!success) return BadRequest("Failed to sync wishlist");

            return Ok(new { message = "Wishlist synced successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
            {
                return Unauthorized();
            }

            var bookIds = await _wishlistService.GetWishlistBookIdsAsync(userId);
            return Ok(bookIds);
        }
    }
}


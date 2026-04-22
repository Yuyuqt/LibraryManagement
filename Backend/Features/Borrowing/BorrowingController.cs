using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Features.Borrowings
{
    [ApiController]
    [Route("api/borrowings")]
    public class BorrowingController : ControllerBase
    {
        private readonly IBorrowingService _borrowingService;

        public BorrowingController(IBorrowingService borrowingService)
        {
            _borrowingService = borrowingService;
        }

        [HttpPost("borrow")]
        [Authorize]
        public async Task<ActionResult<BorrowingDto>> BorrowBook([FromBody] BorrowRequest request)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

                var userId = Guid.Parse(userIdStr);
                var borrowing = await _borrowingService.BorrowBookAsync(userId, request.BookId);
                return Ok(borrowing);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("return-request/{id}")]
        [Authorize]
        public async Task<ActionResult<BorrowingDto>> RequestReturn(Guid id)
        {
            try
            {
                var borrowing = await _borrowingService.RequestReturnAsync(id);
                return Ok(borrowing);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("return/{id}")]
        [Authorize(Roles = "Librarian")]
        public async Task<ActionResult<BorrowingDto>> ReturnBook(Guid id)
        {
            try
            {
                var borrowing = await _borrowingService.ReturnBookAsync(id);
                return Ok(borrowing);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<BorrowingDto>>> GetMyBorrowings()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);
            var borrowings = await _borrowingService.GetUserBorrowingsAsync(userId);
            return Ok(borrowings);
        }

        [HttpGet]
        [Authorize(Roles = "Librarian")]
        public async Task<ActionResult<IEnumerable<BorrowingDto>>> GetAllBorrowings()
        {
            var borrowings = await _borrowingService.GetAllBorrowingsAsync();
            return Ok(borrowings);
        }
    }
}

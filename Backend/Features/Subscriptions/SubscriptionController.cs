using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Features.Subscriptions
{
    [ApiController]
    [Route("api")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet("memberships")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MembershipDto>>> GetMemberships()
        {
            return Ok(await _subscriptionService.GetMembershipsAsync());
        }

        [HttpGet("subscriptions/me")]
        [Authorize]
        public async Task<ActionResult<SubscriptionDto>> GetMySubscription()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);
            var subscription = await _subscriptionService.GetUserSubscriptionAsync(userId);
            
            if (subscription == null) return NotFound(new { message = "No active subscription found." });
            
            return Ok(subscription);
        }

        [HttpGet("subscriptions/me/all")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetMyAllSubscriptions()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);
            var subscriptions = await _subscriptionService.GetUserAllSubscriptionsAsync(userId);
            return Ok(subscriptions);
        }

        [HttpPost("subscriptions/subscribe")]
        [Authorize]
        public async Task<ActionResult<SubscriptionDto>> Subscribe([FromBody] SubscribeRequest request)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

                var userId = Guid.Parse(userIdStr);
                var subscription = await _subscriptionService.SubscribeUserAsync(userId, request.MembershipId);
                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("subscriptions/user/{userId}")]
        [Authorize(Roles = "Librarian")]
        public async Task<ActionResult<SubscriptionDto>> GetUserSubscription(Guid userId)
        {
            var subscription = await _subscriptionService.GetUserSubscriptionAsync(userId);
            if (subscription == null) return NotFound(new { message = "No active subscription found." });
            
            return Ok(subscription);
        }

        [HttpGet("subscriptions/all")]
        [Authorize(Roles = "Librarian")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetSubscriptions()
        {
            return Ok(await _subscriptionService.GetAllSubscriptionsAsync());
        }

        [HttpPost("subscriptions/admin-subscribe")]
        [Authorize(Roles = "Librarian")]
        public async Task<ActionResult<SubscriptionDto>> AdminSubscribe([FromBody] AdminSubscribeRequest request)
        {
            try
            {
                var subscription = await _subscriptionService.SubscribeUserAsync(request.UserId, request.MembershipId);
                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

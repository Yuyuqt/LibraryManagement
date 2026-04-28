using LibraryManagement.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Backend.Features.Notification;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserNotifications()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
        {
            return Unauthorized();
        }

        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
        {
            return Unauthorized();
        }

        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(count);
    }

    [HttpPost("mark-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
        {
            return Unauthorized();
        }

        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok();
    }

    [HttpPost("subscription-expiry")]
    public async Task<IActionResult> SendSubscriptionExpiryNotification([FromBody] SubscriptionExpiryNotificationRequest request)
    {
        string title = "Subscription Expiry Reminder";
        string body = $"Hello {request.FullName}, your library subscription is expiring in {request.DaysRemaining} days. Please renew it to continue enjoying our services!";

        bool success = await _notificationService.SendAndSaveNotificationAsync(
            request.UserId, 
            request.FcmToken, 
            title, 
            body, 
            "Warning", 
            "/Membership", 
            "Renew Now");

        if (success)
            return Ok(new NotificationResponse { Success = true, Message = "Notification sent and saved successfully" });
        
        return BadRequest(new NotificationResponse { Success = false, Message = "Failed to process notification" });
    }

    [HttpPost("return-reminder")]
    public async Task<IActionResult> SendReturnReminder([FromBody] ReturnReminderNotificationRequest request)
    {
        string title = "Book Return Reminder";
        string body = $"Hello {request.FullName}, the book '{request.BookTitle}' is due for return on {request.DueDate:MMM dd, yyyy}. Please make sure to return it on time!";

        bool success = await _notificationService.SendAndSaveNotificationAsync(
            request.UserId, 
            request.FcmToken, 
            title, 
            body, 
            "Warning", 
            "/Borrowings", 
            "View Loans");

        if (success)
            return Ok(new NotificationResponse { Success = true, Message = "Notification sent and saved successfully" });
        
        return BadRequest(new NotificationResponse { Success = false, Message = "Failed to process notification" });
    }
}


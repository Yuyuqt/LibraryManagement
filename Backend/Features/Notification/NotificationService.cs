using LibraryManagement.Shared.Models;
using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbConnect.Data;
using DbConnect.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Notification;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> SendNotificationAsync(string token, string title, string body, Dictionary<string, string>? data = null)
    {
        var message = new Message()
        {
            Token = token,
            Notification = new FirebaseAdmin.Messaging.Notification()
            {
                Title = title,
                Body = body
            },
            Data = data
        };

       try

      {string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        return !string.IsNullOrEmpty(response);
        }
        catch (Exception ex)
        {
           Console.WriteLine($"Error sending FCM message: {ex.Message}");
           return false;
       }
    }

    public async Task<bool> SendAndSaveNotificationAsync(Guid userId, string? token, string title, string body, string type = "Info", string? actionLink = null, string? actionText = null)
    {
        // 0. Verify User Exists
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            Console.WriteLine($"[NotificationService] Error: User with ID {userId} not found. Cannot save notification.");
            return false;
        }

        // 1. Save to Database
        var notification = new DbConnect.Entities.Notification
        {
            UserId = userId,
            Title = title,
            Message = body,
            Type = type,
            ActionLink = actionLink,
            ActionText = actionText,
            CreatedAt = DateTime.Now,
            IsRead = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // 2. Send via FCM if token exists
        if (!string.IsNullOrEmpty(token))
        {
            var data = new Dictionary<string, string>
            {
                { "type", type }
            };
            if (!string.IsNullOrEmpty(actionLink)) data.Add("actionLink", actionLink);
            if (!string.IsNullOrEmpty(actionText)) data.Add("actionText", actionText);

            return await SendNotificationAsync(token, title, body, data);
        }

        return true;
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                ActionLink = n.ActionLink,
                ActionText = n.ActionText,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        if (unread.Any())
        {
            foreach (var n in unread)
            {
                n.IsRead = true;
            }
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> MarkAsReadAsync(int id)
    {
        var notif = await _context.Notifications.FindAsync(id);
        if (notif == null) return false;

        notif.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }
}


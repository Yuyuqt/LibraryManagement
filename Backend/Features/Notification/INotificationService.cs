using System.Threading.Tasks;
using System.Collections.Generic;

namespace Backend.Features.Notification;

public interface INotificationService
{
    Task<bool> SendNotificationAsync(string token, string title, string body, Dictionary<string, string>? data = null);
    Task<bool> SendAndSaveNotificationAsync(Guid userId, string? token, string title, string body, string type = "Info", string? actionLink = null, string? actionText = null);
    Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId);
    Task<bool> MarkAllAsReadAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
}

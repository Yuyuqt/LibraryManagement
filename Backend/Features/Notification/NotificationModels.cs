namespace Backend.Features.Notification;

public class SubscriptionExpiryNotificationRequest
{
    public Guid UserId { get; set; }
    public string FcmToken { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public int DaysRemaining { get; set; }
}

public class ReturnReminderNotificationRequest
{
    public Guid UserId { get; set; }
    public string FcmToken { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string BookTitle { get; set; } = null!;
    public DateTime DueDate { get; set; }
}

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? ActionLink { get; set; }
    public string? ActionText { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
}

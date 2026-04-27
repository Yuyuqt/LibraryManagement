using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DbConnect.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Notification;

public class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Check once a day

    public NotificationBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    await CheckSubscriptionExpiries(context, notificationService);
                    await CheckBorrowingReminders(context, notificationService);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in NotificationBackgroundService: {ex.Message}");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckSubscriptionExpiries(AppDbContext context, INotificationService notificationService)
    {
        var targetDate = DateTime.Today.AddDays(25);
        
        var expiringSubscriptions = await context.UserSubscriptions
            .Include(s => s.User)
            .Where(s => s.IsActive && s.ExpiryDate.Date <= targetDate)
            .ToListAsync();

        foreach (var sub in expiringSubscriptions)
        {
            // Check if notification already sent TODAY to avoid duplicates on restart
            var alreadySentToday = await context.Notifications.AnyAsync(n => 
                n.UserId == sub.UserId && 
                n.Title == "Subscription Renewal Reminder" && 
                n.CreatedAt >= DateTime.Today);

            if (alreadySentToday) continue;

            string title = "Subscription Renewal Reminder";
            string message = $"Hello {sub.User.FullName}, your library subscription will expire soon ({sub.ExpiryDate:MMM dd, yyyy}). Renew now to avoid interruption!";
            
            await notificationService.SendAndSaveNotificationAsync(
                sub.UserId, 
                sub.User.FcmToken, 
                title, 
                message, 
                "Warning", 
                "/Membership", 
                "Renew Now");
        }
    }

    private async Task CheckBorrowingReminders(AppDbContext context, INotificationService notificationService)
    {
        var targetDate = DateTime.Today.AddDays(10);

        var dueBorrowings = await context.Borrowings
            .Include(b => b.User)
            .Include(b => b.Book)
            .Where(b => b.Status == "Borrowed" && b.DueDate.Date <= targetDate)
            .ToListAsync();

        foreach (var loan in dueBorrowings)
        {
            // Check if notification already sent for this specific book TODAY
            var alreadySentToday = await context.Notifications.AnyAsync(n => 
                n.UserId == loan.UserId && 
                n.Title == "Book Return Reminder" && 
                n.Message.Contains(loan.Book.Title) &&
                n.CreatedAt >= DateTime.Today);

            if (alreadySentToday) continue;

            string title = "Book Return Reminder";
            string message = $"Hello {loan.User.FullName}, the book '{loan.Book.Title}' is due soon ({loan.DueDate:MMM dd, yyyy}). Please remember to return it on time!";
            
            await notificationService.SendAndSaveNotificationAsync(
                loan.UserId, 
                loan.User.FcmToken, 
                title, 
                message, 
                "Info", 
                "/Borrowings", 
                "View My Loans");
        }
    }
}

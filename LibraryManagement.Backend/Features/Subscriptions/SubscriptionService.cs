using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Backend.Features.Subscriptions
{
    public interface ISubscriptionService
    {
        Task<IEnumerable<MembershipDto>> GetMembershipsAsync();
        Task<SubscriptionDto?> GetUserSubscriptionAsync(int userId);
        Task<SubscriptionDto> SubscribeUserAsync(int userId, int membershipId);
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly LibraryManagementContext _context;

        public SubscriptionService(LibraryManagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MembershipDto>> GetMembershipsAsync()
        {
            return await _context.Memberships
                .Select(m => new MembershipDto
                {
                    Id = m.Id,
                    Type = m.Type,
                    MaxBooks = m.MaxBooks,
                    BorrowingDays = m.BorrowingDays,
                    Price = m.Price,
                    DurationMonths = m.DurationMonths
                }).ToListAsync();
        }

        public async Task<SubscriptionDto?> GetUserSubscriptionAsync(int userId)
        {
            var subscription = await _context.UserSubscriptions
                .Include(s => s.Membership)
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (subscription == null) return null;

            return MapToDto(subscription);
        }

        public async Task<SubscriptionDto> SubscribeUserAsync(int userId, int membershipId)
        {
            var membership = await _context.Memberships.FindAsync(membershipId);
            if (membership == null) throw new Exception("Membership plan not found.");

            // Deactivate any existing active subscriptions for this user
            var activeSubscriptions = await _context.UserSubscriptions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();

            foreach (var s in activeSubscriptions)
            {
                s.IsActive = false;
            }

            var subscription = new UserSubscription
            {
                UserId = userId,
                MembershipId = membershipId,
                StartDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(membership.DurationMonths),
                IsActive = true
            };

            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // Explicitly load the membership data for the DTO mapping
            await _context.Entry(subscription).Reference(s => s.Membership).LoadAsync();

            return MapToDto(subscription);
        }

        private static SubscriptionDto MapToDto(UserSubscription subscription)
        {
            return new SubscriptionDto
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                MembershipId = subscription.MembershipId,
                MembershipType = subscription.Membership.Type,
                StartDate = subscription.StartDate,
                ExpiryDate = subscription.ExpiryDate,
                IsActive = subscription.IsActive
            };
        }
    }
}

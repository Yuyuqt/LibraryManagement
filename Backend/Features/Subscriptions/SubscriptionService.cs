using Database.Models;
using Microsoft.EntityFrameworkCore;
using Backend.Features.Loyalty;

namespace Backend.Features.Subscriptions
{
    public interface ISubscriptionService
    {
        Task<IEnumerable<MembershipDto>> GetMembershipsAsync();
        Task<SubscriptionDto?> GetUserSubscriptionAsync(int userId);
        Task<bool> HandleLoyaltyRedemptionAsync(int userId, string rewardId, string id);
        Task<SubscriptionDto> SubscribeUserAsync(int userId, int membershipId);
        Task<IEnumerable<SubscriptionDto>> GetAllSubscriptionsAsync();
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly LibraryManagementContext _context;
        private readonly ILoyaltyService _loyaltyService;

        public SubscriptionService(LibraryManagementContext context, ILoyaltyService loyaltyService)
        {
            _context = context;
            _loyaltyService = loyaltyService;
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
                    DurationMonths = m.DurationMonths,
                    LoyaltyRewardId = m.LoyaltyRewardId
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

        public async Task<bool> HandleLoyaltyRedemptionAsync(int userId, string rewardId, string id)
        {
            // externalUserId matches how other loyalty calls identify users
            string externalUserId = userId.ToString();

            try
            {
                // Attempt to claim the reward via the loyalty service
                var (success, message) = await _loyaltyService.ClaimRewardAsync(externalUserId, rewardId, id);

                if (!success)
                {
                    return false;
                }

                // Optionally update the redemption status in the loyalty system
                await _loyaltyService.UpdateRedemptionStatusAsync(id, "CLAIMED");

                return true;
            }
            catch
            {
                // Swallow exceptions to maintain interface contract; caller can handle false result
                return false;
            }
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
            await _context.Entry(subscription).Reference(s => s.User).LoadAsync();

            // Loyalty Integration: Send SUBSCRIBE event
            string externalUserId = userId.ToString();
            string userMobile = subscription.User?.PhoneNumber ?? "0000000000";
            string userEmail = subscription.User?.Email ?? "No Email";

            await _loyaltyService.ProcessEventAsync(
                externalUserId: externalUserId,
                eventKey: "SUBSCRIBE",
                eventValue: (double)membership.Price,
                referenceId: $"SUB-{subscription.Id}",
                description: $"Purchased Membership: {membership.Type}",
                email: userEmail,
                mobile: userMobile
            );

            return MapToDto(subscription);
        }

        public async Task<IEnumerable<SubscriptionDto>> GetAllSubscriptionsAsync()
        {
            var subscriptions = await _context.UserSubscriptions
                .Include(s => s.Membership)
                .Include(s => s.User)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

            return subscriptions.Select(MapToDto);
        }

        private static SubscriptionDto MapToDto(UserSubscription subscription)
        {
            return new SubscriptionDto
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                MembershipId = subscription.MembershipId,
                MembershipType = subscription.Membership.Type,
                UserEmail = subscription.User?.Email ?? "Unknown",
                UserName = subscription.User?.FullName ?? "Unknown",
                StartDate = subscription.StartDate,
                ExpiryDate = subscription.ExpiryDate,
                IsActive = subscription.IsActive
            };
        }
    }
}

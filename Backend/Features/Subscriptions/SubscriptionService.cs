using DbConnect.Data;
using DbConnect.Entities;
using Microsoft.EntityFrameworkCore;
using Backend.Features.Loyalty;

namespace Backend.Features.Subscriptions
{
    public interface ISubscriptionService
    {
        Task<IEnumerable<MembershipDto>> GetMembershipsAsync();
        Task<SubscriptionDto?> GetUserSubscriptionAsync(Guid userId);
        Task<IEnumerable<SubscriptionDto>> GetUserAllSubscriptionsAsync(Guid userId);
        Task<bool> HandleLoyaltyRedemptionAsync(Guid userId, string rewardId, string rewardName, string redemptionId);
        Task<SubscriptionDto> SubscribeUserAsync(Guid userId, int membershipId);
        Task<IEnumerable<SubscriptionDto>> GetAllSubscriptionsAsync();
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly AppDbContext _context;
        private readonly ILoyaltyService _loyaltyService;

        public SubscriptionService(AppDbContext context, ILoyaltyService loyaltyService)
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
                    RewardId = m.RewardId
                }).ToListAsync();
        }

        public async Task<SubscriptionDto?> GetUserSubscriptionAsync(Guid userId)
        {
            var subscription = await _context.UserSubscriptions
                .Include(s => s.Membership)
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (subscription == null) return null;

            return MapToDto(subscription);
        }

        public async Task<IEnumerable<SubscriptionDto>> GetUserAllSubscriptionsAsync(Guid userId)
        {
            var subscriptions = await _context.UserSubscriptions
                .Include(s => s.Membership)
                .Include(s => s.User)
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderBy(s => s.StartDate)
                .ToListAsync();

            return subscriptions.Select(MapToDto);
        }

        public async Task<bool> HandleLoyaltyRedemptionAsync(Guid userId, string rewardId, string rewardName, string redemptionId)
        {
            try
            {
                // Load all memberships into memory (small collection)
                var allMemberships = await _context.Memberships.ToListAsync();
                
                Membership? membership = null;

                // 1. First try exact match by RewardId (most precise)
                if (!string.IsNullOrEmpty(rewardId))
                {
                    membership = allMemberships.FirstOrDefault(m => m.RewardId == rewardId);
                }

                // 2. Fallback: in-memory string matching on rewardName vs membership Type
                //    e.g. "Free Monthly Basic Membership" contains words "Basic" and "Monthly"
                if (membership == null && !string.IsNullOrEmpty(rewardName))
                {
                    var normalizedRewardName = rewardName.ToLowerInvariant();
                    membership = allMemberships.FirstOrDefault(m =>
                    {
                        var typeWords = m.Type.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        return typeWords.All(w => normalizedRewardName.Contains(w));
                    });
                }

                if (membership == null)
                {
                    System.Diagnostics.Debug.WriteLine($"No membership matched for rewardId='{rewardId}', rewardName='{rewardName}'");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Matched membership '{membership.Type}' (Id={membership.Id}) for rewardName='{rewardName}'");
                
                // 3. Grant the membership (will be queued if they already have one)
                await SubscribeUserAsync(userId, membership.Id);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HandleLoyaltyRedemptionAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<SubscriptionDto> SubscribeUserAsync(Guid userId, int membershipId)
        {
            var membership = await _context.Memberships.FindAsync(membershipId);
            if (membership == null) throw new Exception("Membership plan not found.");

            // Find the latest expiry date for this user to enable queuing
            // We look at all active or future subscriptions
            var latestSubscription = await _context.UserSubscriptions
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.ExpiryDate)
                .FirstOrDefaultAsync();

            DateTime startDate = DateTime.UtcNow;
            
            // If there's an existing subscription that expires in the future, start after it
            if (latestSubscription != null && latestSubscription.ExpiryDate > startDate)
            {
                startDate = latestSubscription.ExpiryDate;
            }

            var subscription = new UserSubscription
            {
                UserId = userId,
                MembershipId = membershipId,
                StartDate = startDate,
                ExpiryDate = startDate.AddMonths(membership.DurationMonths),
                IsActive = true
            };

            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // Explicitly load context for DTO
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

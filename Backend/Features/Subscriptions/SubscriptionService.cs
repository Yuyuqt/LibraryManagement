using LibraryManagement.Shared.Models;
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
        Task<SubscriptionDto> SubscribeUserAsync(Guid userId, int membershipId, string? redemptionId = null);
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
                var allMemberships = await _context.Memberships.ToListAsync();
                Membership? membership = null;

                if (!string.IsNullOrEmpty(rewardId))
                {
                    membership = allMemberships.FirstOrDefault(m => m.RewardId == rewardId);
                }

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

                await SubscribeUserAsync(userId, membership.Id, redemptionId);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HandleLoyaltyRedemptionAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<SubscriptionDto> SubscribeUserAsync(Guid userId, int membershipId, string? redemptionId = null)
        {
            var membership = await _context.Memberships.FindAsync(membershipId);
            if (membership == null) throw new Exception("Membership plan not found.");

            var latestSubscription = await _context.UserSubscriptions
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.ExpiryDate)
                .FirstOrDefaultAsync();

            DateTime startDate = DateTime.UtcNow;
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
                IsActive = true,
                ExternalRedemptionId = redemptionId,
                Status = "Active"
            };

            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            await _context.Entry(subscription).Reference(s => s.Membership).LoadAsync();
            await _context.Entry(subscription).Reference(s => s.User).LoadAsync();

            await _loyaltyService.ProcessEventAsync(
                externalUserId: userId.ToString(),
                eventKey: "SUBSCRIBE",
                eventValue: (double)membership.Price,
                referenceId: $"SUB-{subscription.Id}",
                description: $"Purchased Membership: {membership.Type}",
                email: subscription.User?.Email ?? "No Email",
                mobile: subscription.User?.PhoneNumber ?? "0000000000"
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
                IsActive = subscription.IsActive,
                ExternalRedemptionId = subscription.ExternalRedemptionId
            };
        }
    }
}

using LibraryManagement.Shared.Models;
using DbConnect.Data;
using DbConnect.Entities;
using Microsoft.EntityFrameworkCore;
using Backend.Features.Loyalty;
using Backend.Features.Wallet;


namespace Backend.Features.Subscriptions
{
    public interface ISubscriptionService
    {
        Task<IEnumerable<MembershipDto>> GetMembershipsAsync();
        Task<SubscriptionDto?> GetUserSubscriptionAsync(Guid userId);
        Task<IEnumerable<SubscriptionDto>> GetUserAllSubscriptionsAsync(Guid userId);
        Task<bool> HandleLoyaltyRedemptionAsync(Guid userId, string rewardId, string rewardName, string redemptionId);
        Task<SubscriptionDto> SubscribeUserAsync(Guid userId, int membershipId, string? redemptionId = null, string status = "Active", string paymentMethod = "Cash");
        Task<IEnumerable<SubscriptionDto>> GetAllSubscriptionsAsync();
        Task<IEnumerable<SubscriptionDto>> GetPendingSubscriptionsAsync();
        Task<SubscriptionDto> CreatePendingSubscriptionAsync(Guid userId, int membershipId);
        Task<bool> ApproveSubscriptionAsync(Guid subscriptionId, bool approve);
        Task<SubscriptionUpgradePreviewDto> GetUpgradePreviewAsync(Guid userId, int newMembershipId);
        Task<SubscriptionDto> SubscribeWithWalletAsync(Guid userId, int membershipId);
    }



    public class SubscriptionService : ISubscriptionService
    {
        private readonly AppDbContext _context;
        private readonly ILoyaltyService _loyaltyService;
        private readonly IWalletService _walletService;

        public SubscriptionService(AppDbContext context, ILoyaltyService loyaltyService, IWalletService walletService)
        {
            _context = context;
            _loyaltyService = loyaltyService;
            _walletService = walletService;
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

        public async Task<SubscriptionDto> SubscribeUserAsync(Guid userId, int membershipId, string? redemptionId = null, string status = "Active", string paymentMethod = "Cash")

        {
            var membership = await _context.Memberships.FindAsync(membershipId);
            if (membership == null) throw new Exception("Membership plan not found.");

            // Cooldown check: Max 1 active and 1 queued subscription
            var activeAndQueued = await _context.UserSubscriptions
                .Where(s => s.UserId == userId && s.IsActive && s.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();

            if (activeAndQueued.Count >= 2)
            {
                throw new Exception("You already have an active and a queued subscription. Please wait until one expires.");
            }

            var currentActive = activeAndQueued
                .Where(s => s.StartDate <= DateTime.UtcNow)
                .OrderByDescending(s => s.ExpiryDate)
                .FirstOrDefault();

            DateTime startDate = DateTime.UtcNow;
            decimal discount = 0;

            if (currentActive != null)
            {
                // If it's the same membership, we queue it
                if (currentActive.MembershipId == membershipId)
                {
                    startDate = currentActive.ExpiryDate;
                }
                else
                {
                    // If it's a different membership, we treat it as an upgrade/change
                    // Calculate discount and deactivate old one
                    var preview = await GetUpgradePreviewAsync(userId, membershipId);
                    discount = preview.DiscountAmount;
                    currentActive.IsActive = false;
                    currentActive.Status = "Upgraded";
                    startDate = DateTime.UtcNow;
                }
            }

            var subscription = new UserSubscription
            {
                UserId = userId,
                MembershipId = membershipId,
                StartDate = startDate,
                ExpiryDate = startDate.AddMonths(membership.DurationMonths),
                IsActive = status == "Active",
                ExternalRedemptionId = redemptionId,
                Status = status,
                PaymentMethod = paymentMethod
            };


            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            await _context.Entry(subscription).Reference(s => s.Membership).LoadAsync();
            await _context.Entry(subscription).Reference(s => s.User).LoadAsync();

            // Only award loyalty points for the amount actually paid (Price - Discount)
            decimal paidAmount = membership.Price - discount;
            if (paidAmount < 0) paidAmount = 0;

            if (status == "Active")
            {
                await _loyaltyService.ProcessEventAsync(
                    externalUserId: userId.ToString(),
                    eventKey: "SUBSCRIBE",
                    eventValue: (double)paidAmount,
                    referenceId: $"SUB-{subscription.Id}",
                    description: $"Purchased Membership: {membership.Type} (Discount applied: {discount})",
                    email: subscription.User?.Email ?? "No Email",
                    mobile: subscription.User?.PhoneNumber ?? "0000000000"
                );
            }


            return MapToDto(subscription);
        }

        public async Task<SubscriptionUpgradePreviewDto> GetUpgradePreviewAsync(Guid userId, int newMembershipId)
        {
            var newMembership = await _context.Memberships.FindAsync(newMembershipId);
            if (newMembership == null) return new SubscriptionUpgradePreviewDto { CanUpgrade = false, Message = "Plan not found" };

            var currentSubscription = await _context.UserSubscriptions
                .Include(s => s.Membership)
                .Where(s => s.UserId == userId && s.IsActive && s.StartDate <= DateTime.UtcNow && s.ExpiryDate > DateTime.UtcNow)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (currentSubscription == null)
            {
                return new SubscriptionUpgradePreviewDto
                {
                    OriginalPrice = newMembership.Price,
                    DiscountAmount = 0,
                    FinalPrice = newMembership.Price,
                    CanUpgrade = true
                };
            }

            if (currentSubscription.MembershipId == newMembershipId)
            {
                return new SubscriptionUpgradePreviewDto
                {
                    OriginalPrice = newMembership.Price,
                    DiscountAmount = 0,
                    FinalPrice = newMembership.Price,
                    CanUpgrade = true,
                    Message = "Same plan selected. This will be queued after your current subscription."
                };
            }

            // Calculate unused value
            var totalDuration = (currentSubscription.ExpiryDate - currentSubscription.StartDate).TotalDays;
            var daysRemaining = (currentSubscription.ExpiryDate - DateTime.UtcNow).TotalDays;
            
            if (daysRemaining < 0) daysRemaining = 0;

            decimal dailyRate = currentSubscription.Membership.Price / (decimal)totalDuration;
            decimal unusedValue = dailyRate * (decimal)daysRemaining;

            // Cap discount at new price
            decimal discount = Math.Min(unusedValue, newMembership.Price);

            return new SubscriptionUpgradePreviewDto
            {
                OriginalPrice = newMembership.Price,
                DiscountAmount = Math.Round(discount, 2),
                FinalPrice = Math.Round(newMembership.Price - discount, 2),
                CanUpgrade = true,
                Message = $"Pro-rated discount applied for your current {currentSubscription.Membership.Type} plan."
            };
        }

        public async Task<SubscriptionDto> SubscribeWithWalletAsync(Guid userId, int membershipId)
        {
            var preview = await GetUpgradePreviewAsync(userId, membershipId);
            if (!preview.CanUpgrade) throw new Exception(preview.Message ?? "Cannot purchase this plan.");

            var balance = await _walletService.GetBalanceAsync(userId);
            if (balance < preview.FinalPrice)
            {
                throw new Exception($"Insufficient balance. You need {preview.FinalPrice - balance:N0} more MMK.");
            }

            var membership = await _context.Memberships.FindAsync(membershipId);
            
            // Deduct from wallet
            bool deducted = await _walletService.DeductAsync(userId, preview.FinalPrice, $"Subscription: {membership!.Type}");
            if (!deducted) throw new Exception("Failed to deduct from wallet.");

            // Create subscription
            return await SubscribeUserAsync(userId, membershipId, status: "Active", paymentMethod: "Wallet");
        }

        public async Task<IEnumerable<SubscriptionDto>> GetPendingSubscriptionsAsync()
        {
            var pending = await _context.UserSubscriptions
                .Include(s => s.Membership)
                .Include(s => s.User)
                .Where(s => s.Status == "PendingApproval")
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

            return pending.Select(MapToDto);
        }

        public async Task<SubscriptionDto> CreatePendingSubscriptionAsync(Guid userId, int membershipId)
        {
            // Similar logic to SubscribeUserAsync but set to PendingApproval
            return await SubscribeUserAsync(userId, membershipId, status: "PendingApproval", paymentMethod: "Cash");
        }

        public async Task<bool> ApproveSubscriptionAsync(Guid subscriptionId, bool approve)
        {
            var subscription = await _context.UserSubscriptions
                .Include(s => s.Membership)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);

            if (subscription == null) return false;

            if (approve)
            {
                subscription.Status = "Active";
                subscription.IsActive = true;
                
                // Award points now
                decimal paidAmount = 0;
                if (subscription.Membership != null)
                {
                    decimal discount = 0; // Simplified for approval flow for now
                    paidAmount = subscription.Membership.Price - discount;
                }
                
                await _loyaltyService.ProcessEventAsync(
                    externalUserId: subscription.UserId.ToString(),
                    eventKey: "SUBSCRIBE",
                    eventValue: (double)paidAmount,
                    referenceId: $"SUB-{subscription.Id}",
                    description: $"Membership Approved: {subscription.Membership?.Type ?? "Unknown Plan"}",
                    email: subscription.User?.Email ?? "No Email",
                    mobile: subscription.User?.PhoneNumber ?? "0000000000"
                );
            }
            else
            {
                subscription.Status = "Rejected";
                subscription.IsActive = false;
            }

            await _context.SaveChangesAsync();
            return true;
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
                MembershipType = subscription.Membership?.Type ?? "Unknown",
                UserEmail = subscription.User?.Email ?? "Unknown",
                UserName = subscription.User?.FullName ?? "Unknown",
                StartDate = subscription.StartDate,
                ExpiryDate = subscription.ExpiryDate,
                IsActive = subscription.IsActive,
                Status = subscription.Status,
                PaymentMethod = subscription.PaymentMethod,
                ExternalRedemptionId = subscription.ExternalRedemptionId
            };

        }
    }
}

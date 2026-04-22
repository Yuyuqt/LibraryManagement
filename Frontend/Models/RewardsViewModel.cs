using Frontend.Models.Dtos;

namespace Frontend.Models
{
    // Member rewards page view model
    public class RewardsViewModel
    {
        public IEnumerable<LoyaltyRewardDto> Rewards { get; set; } = Enumerable.Empty<LoyaltyRewardDto>();
        public LoyaltyAccountDto? Account { get; set; }
        public IEnumerable<LoyaltyRedemptionDto> PendingRedemptions { get; set; } = Enumerable.Empty<LoyaltyRedemptionDto>();
        public IEnumerable<LoyaltyRedemptionDto> RedemptionsHistory { get; set; } = Enumerable.Empty<LoyaltyRedemptionDto>();
        public IEnumerable<PointHistoryEntryDto> PointsHistory { get; set; } = Enumerable.Empty<PointHistoryEntryDto>();
    }

    // Librarian rewards management view model
    public class LibrarianRewardsViewModel
    {
        public IEnumerable<LoyaltyRedemptionDto> PendingRedemptions { get; set; } = Enumerable.Empty<LoyaltyRedemptionDto>();
        public IEnumerable<UserPointsHistoryDto> AllMembersHistory { get; set; } = Enumerable.Empty<UserPointsHistoryDto>();
    }
}

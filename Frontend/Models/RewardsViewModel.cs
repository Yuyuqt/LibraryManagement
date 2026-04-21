using Frontend.Models.Dtos;

namespace Frontend.Models
{
    public class RewardsViewModel
    {
        public IEnumerable<LoyaltyRewardDto> Rewards { get; set; } = Enumerable.Empty<LoyaltyRewardDto>();
        public LoyaltyAccountDto? Account { get; set; }
    }
}

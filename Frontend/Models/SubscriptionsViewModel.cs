using Frontend.Models.Dtos;

namespace Frontend.Models
{
    public class SubscriptionsViewModel
    {
        public List<MembershipDto> AvailableMemberships { get; set; } = new();
        public List<SubscriptionDto> ActiveSubscriptions { get; set; } = new();
    }
}

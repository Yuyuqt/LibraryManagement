using BlazorFrontend.Models.Dtos;

namespace BlazorFrontend.Models
{
    public class SubscriptionsViewModel
    {
        public List<MembershipDto> AvailableMemberships { get; set; } = new();
        public List<SubscriptionDto> ActiveSubscriptions { get; set; } = new();
    }
}

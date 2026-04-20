using Frontend.Models.Dtos;
using System.Collections.Generic;

namespace Frontend.Models
{
    public class MemberDetailsViewModel
    {
        public UserDto User { get; set; } = new();
        public SubscriptionDto? CurrentSubscription { get; set; }
        public List<MembershipDto> AvailableMemberships { get; set; } = new();
    }
}

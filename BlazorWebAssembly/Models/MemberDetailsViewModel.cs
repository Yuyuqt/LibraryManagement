using LibraryManagement.Shared.Models;
using System.Collections.Generic;

namespace BlazorWebAssembly.Models
{
    public class MemberDetailsViewModel
    {
        public UserDto User { get; set; } = new();
        public SubscriptionDto? CurrentSubscription { get; set; }
        public List<MembershipDto> AvailableMemberships { get; set; } = new();
    }
}

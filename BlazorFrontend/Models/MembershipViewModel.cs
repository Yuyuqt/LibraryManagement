using LibraryManagement.Shared.Models;
using System.Collections.Generic;

namespace BlazorFrontend.Models
{
    public class MembershipViewModel
    {
        public SubscriptionDto? CurrentSubscription { get; set; }
        public List<MembershipDto> AvailableMemberships { get; set; } = new List<MembershipDto>();
        public List<SubscriptionDto> QueuedMemberships { get; set; } = new List<SubscriptionDto>();
        public LoyaltyAccountDto? LoyaltyAccount { get; set; }
        public Dictionary<string, double> RewardPointCosts { get; set; } = new Dictionary<string, double>();
    }
}


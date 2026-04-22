using System;
using System.Collections.Generic;

namespace DbConnect.Entities;

public partial class Membership
{
    public int Id { get; set; }

    public string Type { get; set; } = null!;

    public int MaxBooks { get; set; }

    public int BorrowingDays { get; set; }

    public decimal Price { get; set; }

    public int DurationMonths { get; set; }

    public string? RewardId { get; set; }

    public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}

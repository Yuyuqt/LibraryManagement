using System;
using System.Collections.Generic;

namespace DbConnect.Entities;

public partial class UserSubscription
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int MembershipId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public bool IsActive { get; set; }

    public string Status { get; set; } = null!;

    public string? ExternalRedemptionId { get; set; }

    public virtual Membership Membership { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class UserSubscription
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int MembershipId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public bool IsActive { get; set; }

    public virtual Membership Membership { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

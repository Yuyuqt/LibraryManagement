using System;
using System.Collections.Generic;

namespace DbConnect.Entities;

public partial class WishlistItem
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public int BookId { get; set; }

    public DateTime AddedAt { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

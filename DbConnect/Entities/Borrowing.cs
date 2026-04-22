using System;
using System.Collections.Generic;

namespace DbConnect.Entities;

public partial class Borrowing
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int BookId { get; set; }

    public DateTime BorrowDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public string Status { get; set; } = null!;

    public decimal FineAmount { get; set; }

    public bool IsFinePaid { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

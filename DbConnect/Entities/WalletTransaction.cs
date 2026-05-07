using System;

namespace DbConnect.Entities;

public class WalletTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = null!; // Deposit, Purchase, Refund
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? ProcessedBy { get; set; } // Librarian ID

    public virtual User User { get; set; } = null!;
}

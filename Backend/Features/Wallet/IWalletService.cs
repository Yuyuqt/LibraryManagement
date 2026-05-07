using LibraryManagement.Shared.Models;

namespace Backend.Features.Wallet
{
    public interface IWalletService
    {
        Task<decimal> GetBalanceAsync(Guid userId);
        Task<IEnumerable<WalletTransactionDto>> GetHistoryAsync(Guid userId);
        Task<bool> TopUpAsync(Guid userId, decimal amount, Guid librarianId, string? description = null);
        Task<bool> DeductAsync(Guid userId, decimal amount, string description, Guid? referenceId = null);
    }

    public class WalletTransactionDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

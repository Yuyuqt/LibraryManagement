using DbConnect.Data;
using DbConnect.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Wallet
{
    public class WalletService : IWalletService
    {
        private readonly AppDbContext _context;

        public WalletService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetBalanceAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.Balance ?? 0;
        }

        public async Task<IEnumerable<WalletTransactionDto>> GetHistoryAsync(Guid userId)
        {
            return await _context.WalletTransactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new WalletTransactionDto
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    Type = t.Type,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> TopUpAsync(Guid userId, decimal amount, Guid librarianId, string? description = null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Balance += amount;

            var transaction = new WalletTransaction
            {
                UserId = userId,
                Amount = amount,
                Type = "Deposit",
                Description = description ?? "Cash Deposit",
                ProcessedBy = librarianId
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeductAsync(Guid userId, decimal amount, string description, Guid? referenceId = null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Balance < amount) return false;

            user.Balance -= amount;

            var transaction = new WalletTransaction
            {
                UserId = userId,
                Amount = -amount, // Negative for deduction
                Type = "Purchase",
                Description = description
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

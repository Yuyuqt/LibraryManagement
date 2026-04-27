using DbConnect.Data;
using DbConnect.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Features.Wishlist
{
    public interface IWishlistService
    {
        Task<bool> SyncWishlistAsync(Guid userId, List<int> bookIds);
        Task<List<int>> GetWishlistBookIdsAsync(Guid userId);
    }

    public class WishlistService : IWishlistService
    {
        private readonly AppDbContext _context;

        public WishlistService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SyncWishlistAsync(Guid userId, List<int> bookIds)
        {
            // 1. Remove existing wishlist items for this user
            var existing = await _context.WishlistItems
                .Where(w => w.UserId == userId)
                .ToListAsync();
            
            _context.WishlistItems.RemoveRange(existing);

            // 2. Add new wishlist items
            var newItems = bookIds.Select(bookId => new WishlistItem
            {
                UserId = userId,
                BookId = bookId,
                AddedAt = DateTime.UtcNow
            });

            await _context.WishlistItems.AddRangeAsync(newItems);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<int>> GetWishlistBookIdsAsync(Guid userId)
        {
            return await _context.WishlistItems
                .Where(w => w.UserId == userId)
                .Select(w => w.BookId)
                .ToListAsync();
        }
    }
}

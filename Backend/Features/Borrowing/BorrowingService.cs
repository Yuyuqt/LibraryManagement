using DbConnect.Data;
using DbConnect.Entities;
using Microsoft.EntityFrameworkCore;
using Backend.Features.Loyalty;

namespace Backend.Features.Borrowings
{
    public interface IBorrowingService
    {
        Task<BorrowingDto> BorrowBookAsync(Guid userId, int bookId);
        Task<BorrowingDto> RequestReturnAsync(Guid borrowingId);
        Task<BorrowingDto> ReturnBookAsync(Guid borrowingId);
        Task<IEnumerable<BorrowingDto>> GetUserBorrowingsAsync(Guid userId);
        Task<IEnumerable<BorrowingDto>> GetAllBorrowingsAsync();
    }

    public class BorrowingService : IBorrowingService
    {
        private readonly AppDbContext _context;
        private readonly ILoyaltyService _loyaltyService;
        private const decimal FinePerDay = 500;

        public BorrowingService(AppDbContext context, ILoyaltyService loyaltyService)
        {
            _context = context;
            _loyaltyService = loyaltyService;
        }

        public async Task<BorrowingDto> BorrowBookAsync(Guid userId, int bookId)
        {
            // 1. Check for Active Membership
            var subscription = await _context.UserSubscriptions
                .Include(s => s.Membership)
                .Where(s => s.UserId == userId && s.IsActive && s.ExpiryDate > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (subscription == null)
            {
                throw new Exception("Active membership required to borrow books.");
            }

            // 2. Check for Overdue Books or Unpaid Fines
            var hasBlockers = await _context.Borrowings
                .AnyAsync(b => b.UserId == userId && 
                    ((b.Status == "Borrowed" && b.DueDate < DateTime.UtcNow) || (b.FineAmount > 0 && !b.IsFinePaid)));

            if (hasBlockers)
            {
                throw new Exception("Borrowing blocked: You have overdue books or unpaid fines.");
            }

            // 3. Check MaxBooks limit
            var activeBorrowCount = await _context.Borrowings
                .CountAsync(b => b.UserId == userId && b.Status == "Borrowed");

            if (activeBorrowCount >= subscription.Membership.MaxBooks)
            {
                throw new Exception($"Borrowing limit reached: You can only borrow {subscription.Membership.MaxBooks} books at a time.");
            }

            // 4. Check Book Availability
            var book = await _context.Books.FindAsync(bookId);
            if (book == null || !book.IsActive || book.AvailableCopies <= 0)
            {
                throw new Exception("Book is currently unavailable.");
            }

            // 5. Create Borrowing Record
            var borrowing = new Borrowing
            {
                UserId = userId,
                BookId = bookId,
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(subscription.Membership.BorrowingDays),
                Status = "Borrowed",
                FineAmount = 0,
                IsFinePaid = false
            };

            // 6. Update Book Stock
            book.AvailableCopies--;
            if (book.AvailableCopies == 0)
            {
                book.Status = "Out Of Stock";
            }

            _context.Borrowings.Add(borrowing);
            await _context.SaveChangesAsync();

            // Load relations for mapping
            await _context.Entry(borrowing).Reference(b => b.Book).LoadAsync();
            await _context.Entry(borrowing).Reference(b => b.User).LoadAsync();



            return MapToDto(borrowing);
        }

        public async Task<BorrowingDto> RequestReturnAsync(Guid borrowingId)
        {
            var borrowing = await _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == borrowingId);

            if (borrowing == null) throw new Exception("Borrowing record not found.");
            if (borrowing.Status != "Borrowed") throw new Exception("Only borrowed books can be requested for return.");

            borrowing.Status = "PendingReturn";
            await _context.SaveChangesAsync();
            return MapToDto(borrowing);
        }

        public async Task<BorrowingDto> ReturnBookAsync(Guid borrowingId)
        {
            var borrowing = await _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == borrowingId);

            if (borrowing == null) throw new Exception("Borrowing record not found.");
            if (borrowing.Status == "Returned") throw new Exception("Book has already been returned.");

            var now = DateTime.UtcNow;
            borrowing.ReturnDate = now;
            borrowing.Status = "Returned";

            // Calculate Fine
            if (now > borrowing.DueDate)
            {
                var overdueDays = (now.Date - borrowing.DueDate.Date).Days;
                if (overdueDays > 0)
                {
                    borrowing.FineAmount = overdueDays * FinePerDay;
                }
            }

            // Update Book Stock
            var book = borrowing.Book;
            book.AvailableCopies++;
            book.Status = "Available";

            await _context.SaveChangesAsync();

            // Loyalty Integration: Send RETURN event
            string externalUserId = borrowing.UserId.ToString();
            string userMobile = borrowing.User?.PhoneNumber ?? "0000000000";
            string userEmail = borrowing.User?.Email ?? "No Email";
            
            await _loyaltyService.ProcessEventAsync(
                externalUserId: externalUserId,
                eventKey: "RETURN",
                eventValue: 0,
                referenceId: $"RET-{borrowing.Id}",
                description: $"Returned Book: {book?.Title}",
                email: userEmail,
                mobile: userMobile
            );

            return MapToDto(borrowing);
        }

        public async Task<IEnumerable<BorrowingDto>> GetUserBorrowingsAsync(Guid userId)
        {
            var borrowings = await _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.User)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BorrowDate)
                .ToListAsync();

            return borrowings.Select(MapToDto);
        }

        public async Task<IEnumerable<BorrowingDto>> GetAllBorrowingsAsync()
        {
            var borrowings = await _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.User)
                .OrderByDescending(b => b.BorrowDate)
                .ToListAsync();

            return borrowings.Select(MapToDto);
        }

        private static BorrowingDto MapToDto(Borrowing b)
        {
            return new BorrowingDto
            {
                Id = b.Id,
                UserId = b.UserId,
                UserEmail = b.User?.Email ?? "Unknown",
                BookId = b.BookId,
                BookTitle = b.Book?.Title ?? "Unknown",
                BorrowDate = b.BorrowDate,
                DueDate = b.DueDate,
                ReturnDate = b.ReturnDate,
                Status = b.Status,
                FineAmount = b.FineAmount,
                IsFinePaid = b.IsFinePaid
            };
        }
    }
}

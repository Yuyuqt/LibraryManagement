using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Backend.Features.Books
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllBooksAsync();
        Task<BookDto?> GetBookByIdAsync(int id);
        Task<BookDto> CreateBookAsync(BookCreateRequest request);
        Task<BookDto?> UpdateBookAsync(int id, BookUpdateRequest request);
        Task<bool> DeleteBookAsync(int id);
    }

    public class BookService : IBookService
    {
        private readonly LibraryManagementContext _context;

        public BookService(LibraryManagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
        {
            return await _context.Books
                .Where(b => b.IsActive)
                .Select(b => MapToDto(b))
                .ToListAsync();
        }

        public async Task<BookDto?> GetBookByIdAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null || !book.IsActive) return null;
            return MapToDto(book);
        }

        public async Task<BookDto> CreateBookAsync(BookCreateRequest request)
        {
            if (await _context.Books.AnyAsync(b => b.Isbn == request.Isbn))
            {
                throw new Exception("A book with this ISBN already exists.");
            }

            var book = new Book
            {
                Title = request.Title,
                Isbn = request.Isbn,
                Author = request.Author,
                Description = request.Description,
                TotalCopies = request.TotalCopies,
                AvailableCopies = request.TotalCopies,
                Status = request.TotalCopies > 0 ? "Available" : "Out Of Stock",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return MapToDto(book);
        }

        public async Task<BookDto?> UpdateBookAsync(int id, BookUpdateRequest request)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null || !book.IsActive) return null;

            book.Title = request.Title;
            book.Author = request.Author;
            book.Description = request.Description;
            
            // Basic logic to sync availability when total copies change
            int difference = request.TotalCopies - book.TotalCopies;
            book.TotalCopies = request.TotalCopies;
            book.AvailableCopies += difference;

            if (book.AvailableCopies < 0) book.AvailableCopies = 0;
            book.Status = book.AvailableCopies > 0 ? "Available" : "Out Of Stock";
            
            book.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(book);
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;

            book.IsActive = false;
            book.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private static BookDto MapToDto(Book book)
        {
            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Isbn = book.Isbn,
                Author = book.Author,
                Status = book.Status,
                IsActive = book.IsActive,
                Description = book.Description,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt
            };
        }
    }

    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class BookCreateRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TotalCopies { get; set; }
    }

    public class BookUpdateRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TotalCopies { get; set; }
    }
}

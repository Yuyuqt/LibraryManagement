using DbConnect.Data;
using DbConnect.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Books
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllBooksAsync(int? categoryId = null);
        Task<BookDto?> GetBookByIdAsync(int id);
        Task<BookDto> CreateBookAsync(BookCreateRequest request);
        Task<BookDto?> UpdateBookAsync(int id, BookUpdateRequest request);
        Task<bool> DeleteBookAsync(int id);
    }

    public class BookService : IBookService
    {
        private readonly AppDbContext _context;

        public BookService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync(int? categoryId = null)
        {
            var query = _context.Books
                .Include(b => b.Categories)
                .Where(b => b.IsActive);

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.Categories.Any(c => c.Id == categoryId.Value));
            }

            var books = await query.ToListAsync();
            return books.Select(b => MapToDto(b));
        }

        public async Task<BookDto?> GetBookByIdAsync(int id)
        {
            var book = await _context.Books
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(b => b.Id == id);
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

            // Attach selected categories
            if (request.CategoryIds != null && request.CategoryIds.Any())
            {
                var categories = await _context.Categories
                    .Where(c => request.CategoryIds.Contains(c.Id))
                    .ToListAsync();
                foreach (var cat in categories)
                    book.Categories.Add(cat);
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return MapToDto(book);
        }

        public async Task<BookDto?> UpdateBookAsync(int id, BookUpdateRequest request)
        {
            var book = await _context.Books
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(b => b.Id == id);
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

            // Sync categories: replace existing with new selection
            book.Categories.Clear();
            if (request.CategoryIds != null && request.CategoryIds.Any())
            {
                var categories = await _context.Categories
                    .Where(c => request.CategoryIds.Contains(c.Id))
                    .ToListAsync();
                foreach (var cat in categories)
                    book.Categories.Add(cat);
            }

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
                UpdatedAt = book.UpdatedAt,
                Categories = book.Categories.Select(c => new BookCategoryDto { Id = c.Id, Name = c.Name }).ToList()
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
        public List<BookCategoryDto> Categories { get; set; } = new();
    }

    public class BookCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class BookCreateRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TotalCopies { get; set; }
        public List<int>? CategoryIds { get; set; }
    }

    public class BookUpdateRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TotalCopies { get; set; }
        public List<int>? CategoryIds { get; set; }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Books
{
    [ApiController]
    [Route("api/books")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks([FromQuery] int? categoryId = null)
        {
            var books = await _bookService.GetAllBooksAsync(categoryId);
            return Ok(books);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<BookDto>> GetBook(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        [HttpPost]
        [Authorize(Roles = "Librarian")]
        public async Task<ActionResult<BookDto>> CreateBook([FromBody] BookCreateRequest request)
        {
            try
            {
                var book = await _bookService.CreateBookAsync(request);
                return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Librarian")]
        public async Task<ActionResult<BookDto>> UpdateBook(int id, [FromBody] BookUpdateRequest request)
        {
            var updatedBook = await _bookService.UpdateBookAsync(id, request);
            if (updatedBook == null) return NotFound();
            return Ok(updatedBook);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var success = await _bookService.DeleteBookAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}

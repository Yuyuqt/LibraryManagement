using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Categories
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        //[Authorize] // Accessible by both Librarian and Member
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("with-books")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategoriesWithBooks()
        {
            var categories = await _categoryService.GetCategoriesWithBooksAsync();
            return Ok(categories);
        }

        [HttpPost]
        //[Authorize(Roles = "Librarian")]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CategoryCreateRequest request)
        {
            try
            {
                var category = await _categoryService.CreateCategoryAsync(request);
                return CreatedAtAction(nameof(GetCategories), category);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = "Librarian")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var success = await _categoryService.DeleteCategoryAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}

using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Backend.Features.Categories
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> CreateCategoryAsync(CategoryCreateRequest request);
        Task<bool> DeleteCategoryAsync(int id);
    }

    public class CategoryService : ICategoryService
    {
        private readonly LibraryManagementContext _context;

        public CategoryService(LibraryManagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
                .ToListAsync();
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryCreateRequest request)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == request.Name))
            {
                throw new Exception("Category with this name already exists.");
            }

            var category = new Category
            {
                Name = request.Name
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryDto { Id = category.Id, Name = category.Name };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            // Note: EF automatically handles removing associations in many-to-many join tables
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CategoryCreateRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}

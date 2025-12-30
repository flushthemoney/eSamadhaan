using eSamadhaan.Application.DTOs.Category;

namespace eSamadhaan.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<int> CreateCategoryAsync(string name, string description);
    Task<object?> GetCategoryByIdAsync(int id);
    Task<IEnumerable<object>> GetAllCategoriesAsync();
    Task UpdateCategoryAsync(int id, string name, string description);
    Task DeleteCategoryAsync(int id);
    Task<bool> CategoryExistsAsync(int id);
}

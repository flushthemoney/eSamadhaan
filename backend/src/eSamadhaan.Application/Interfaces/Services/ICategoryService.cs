using eSamadhaan.Application.DTOs.Category;

namespace eSamadhaan.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequestDto request);
    Task<CategoryDto> GetCategoryByIdAsync(int id);
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task UpdateCategoryAsync(int id, UpdateCategoryRequestDto request);
    Task DeleteCategoryAsync(int id);
    Task<bool> CategoryExistsAsync(int id);
}

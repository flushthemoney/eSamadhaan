using eSamadhaan.Application.DTOs.Category;
using eSamadhaan.Application.Exceptions;
using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Application.Interfaces.Services;
using eSamadhaan.Domain.Entities;

namespace eSamadhaan.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IGrievanceCategoryRepository _categoryRepository;
    private readonly IGrievanceRepository _grievanceRepository;

    public CategoryService(
        IGrievanceCategoryRepository categoryRepository,
        IGrievanceRepository grievanceRepository)
    {
        _categoryRepository = categoryRepository;
        _grievanceRepository = grievanceRepository;
    }

    public async Task<int> CreateCategoryAsync(string name, string description)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("Category name is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ValidationException("Category description is required.");
        }

        // Create category
        var category = new GrievanceCategory
        {
            Name = name,
            Description = description
        };

        var createdCategory = await _categoryRepository.CreateAsync(category);
        return createdCategory.Id;
    }

    public async Task<object?> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        
        if (category == null)
        {
            throw new NotFoundException("Category", id);
        }

        return MapToDto(category);
    }

    public async Task<IEnumerable<object>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(MapToDto);
    }

    public async Task UpdateCategoryAsync(int id, string name, string description)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        
        if (category == null)
        {
            throw new NotFoundException("Category", id);
        }

        // Validate inputs
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("Category name is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ValidationException("Category description is required.");
        }

        category.Name = name;
        category.Description = description;

        await _categoryRepository.UpdateAsync(category);
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        
        if (category == null)
        {
            throw new NotFoundException("Category", id);
        }

        // Check if category has associated grievances
        var grievanceCount = await _grievanceRepository.CountByCategoryAsync(id);
        if (grievanceCount > 0)
        {
            throw new BusinessRuleViolationException($"Cannot delete category with {grievanceCount} associated grievances.");
        }

        await _categoryRepository.DeleteAsync(id);
    }

    public async Task<bool> CategoryExistsAsync(int id)
    {
        return await _categoryRepository.ExistsAsync(id);
    }

    // Private helper methods
    private CategoryDto MapToDto(GrievanceCategory category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}

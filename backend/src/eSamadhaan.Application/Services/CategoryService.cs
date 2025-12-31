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

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequestDto request)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Category name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ValidationException("Category description is required.");
        }

        // Create category
        var category = new GrievanceCategory
        {
            Name = request.Name,
            Description = request.Description
        };

        var createdCategory = await _categoryRepository.CreateAsync(category);
        return await MapToDtoAsync(createdCategory);
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        
        if (category == null)
        {
            throw new NotFoundException("Category", id);
        }

        return await MapToDtoAsync(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var dtos = new List<CategoryDto>();
        
        foreach (var category in categories)
        {
            dtos.Add(await MapToDtoAsync(category));
        }
        
        return dtos;
    }

    public async Task UpdateCategoryAsync(int id, UpdateCategoryRequestDto request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        
        if (category == null)
        {
            throw new NotFoundException("Category", id);
        }

        // Validate inputs
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Category name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ValidationException("Category description is required.");
        }

        category.Name = request.Name;
        category.Description = request.Description;

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
    private async Task<CategoryDto> MapToDtoAsync(GrievanceCategory category)
    {
        var grievanceCount = await _grievanceRepository.CountByCategoryAsync(category.Id);
        
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            GrievanceCount = grievanceCount
        };
    }
}

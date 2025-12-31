using eSamadhaan.Application.DTOs.Category;
using eSamadhaan.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eSamadhaan.API.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Create a new grievance category (SystemAdmin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequestDto request)
    {
        var category = await _categoryService.CreateCategoryAsync(request);

        return CreatedAtAction(
            nameof(GetCategoryById),
            new { id = category.Id },
            category
        );
    }

    /// <summary>
    /// Update an existing grievance category (SystemAdmin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequestDto request)
    {
        await _categoryService.UpdateCategoryAsync(id, request);
        return NoContent();
    }

    /// <summary>
    /// Get all grievance categories
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Get a specific grievance category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        return Ok(category);
    }

    /// <summary>
    /// Delete a grievance category (SystemAdmin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await _categoryService.DeleteCategoryAsync(id);
        return NoContent();
    }
}

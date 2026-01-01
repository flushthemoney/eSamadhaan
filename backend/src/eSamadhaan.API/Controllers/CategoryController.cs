using eSamadhaan.Application.DTOs.Category;
using eSamadhaan.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eSamadhaan.API.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IUserService _userService;

    public CategoryController(ICategoryService categoryService, IUserService userService)
    {
        _categoryService = categoryService;
        _userService = userService;
    }

    /// <summary>
    /// Create a new grievance category
    /// </summary>
    /// <remarks>
    /// SystemAdmin can create categories for any department.
    /// DepartmentOfficers can only create categories for their own department.
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin,DepartmentOfficer")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequestDto request)
    {
        if (!await ValidateDepartmentAccess(request.DepartmentId))
        {
            return Forbid();
        }

        var category = await _categoryService.CreateCategoryAsync(request);

        return CreatedAtAction(
            nameof(GetCategoryById),
            new { id = category.Id },
            category
        );
    }

    /// <summary>
    /// Update an existing grievance category
    /// </summary>
    /// <remarks>
    /// SystemAdmin can update categories for any department.
    /// DepartmentOfficers can only update categories for their own department.
    /// </remarks>
    [HttpPut("{id}")]
    [Authorize(Roles = "SystemAdmin,DepartmentOfficer")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequestDto request)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        
        if (!await ValidateDepartmentAccess(category.DepartmentId))
        {
            return Forbid();
        }

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
    /// Delete a grievance category
    /// </summary>
    /// <remarks>
    /// SystemAdmin can delete categories for any department.
    /// DepartmentOfficers can only delete categories for their own department.
    /// Deletion is prevented if the category has associated grievances.
    /// </remarks>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SystemAdmin,DepartmentOfficer")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        
        if (!await ValidateDepartmentAccess(category.DepartmentId))
        {
            return Forbid();
        }

        await _categoryService.DeleteCategoryAsync(id);
        return NoContent();
    }

    // Helper method to validate department officer access
    private async Task<bool> ValidateDepartmentAccess(int departmentId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return false;
        }

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        // SystemAdmin has access to all departments
        if (userRole == "SystemAdmin")
        {
            return true;
        }

        // DepartmentOfficer can only access their own department
        if (userRole == "DepartmentOfficer")
        {
            var user = await _userService.GetUserByIdAsync(userId);
            return user.DepartmentId.HasValue && user.DepartmentId.Value == departmentId;
        }

        return false;
    }
}

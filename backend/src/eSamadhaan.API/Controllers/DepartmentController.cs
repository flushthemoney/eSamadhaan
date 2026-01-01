using eSamadhaan.Application.DTOs.Category;
using eSamadhaan.Application.DTOs.Department;
using eSamadhaan.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eSamadhaan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SystemAdmin")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly ICategoryService _categoryService;

    public DepartmentController(
        IDepartmentService departmentService,
        ICategoryService categoryService)
    {
        _departmentService = departmentService;
        _categoryService = categoryService;
    }

    /// <summary>
    /// Create a new department
    /// </summary>
    /// <remarks>
    /// Only SystemAdmin can create departments.
    /// </remarks>
    [HttpPost]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequestDto request)
    {
        var departmentId = await _departmentService.CreateDepartmentAsync(
            request.Name,
            request.Description
        );

        return CreatedAtAction(
            nameof(GetDepartmentById),
            new { id = departmentId },
            new
            {
                message = "Department created successfully",
                departmentId = departmentId
            }
        );
    }

    /// <summary>
    /// Update an existing department
    /// </summary>
    /// <remarks>
    /// Only SystemAdmin can update departments.
    /// </remarks>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentRequestDto request)
    {
        await _departmentService.UpdateDepartmentAsync(
            id,
            request.Name,
            request.Description
        );

        return Ok(new
        {
            message = "Department updated successfully",
            departmentId = id
        });
    }

    /// <summary>
    /// Get all departments
    /// </summary>
    /// <remarks>
    /// Only SystemAdmin can view all departments.
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> GetAllDepartments()
    {
        var departments = await _departmentService.GetAllDepartmentsAsync();
        return Ok(departments);
    }

    /// <summary>
    /// Get a department by ID
    /// </summary>
    /// <remarks>
    /// Only SystemAdmin can view department details.
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDepartmentById(int id)
    {
        var department = await _departmentService.GetDepartmentByIdAsync(id);
        return Ok(department);
    }

    /// <summary>
    /// Delete a department
    /// </summary>
    /// <remarks>
    /// Only SystemAdmin can delete departments.
    /// Deletion is prevented if the department has associated grievances.
    /// </remarks>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        await _departmentService.DeleteDepartmentAsync(id);
        
        return Ok(new
        {
            message = "Department deleted successfully",
            departmentId = id
        });
    }

    /// <summary>
    /// Get all categories for a specific department
    /// </summary>
    /// <remarks>
    /// All authenticated users can view department categories.
    /// </remarks>
    [HttpGet("{departmentId}/categories")]
    [Authorize]
    public async Task<IActionResult> GetDepartmentCategories(int departmentId)
    {
        var categories = await _categoryService.GetCategoriesByDepartmentAsync(departmentId);
        return Ok(categories);
    }

    /// <summary>
    /// Create a category for a specific department
    /// </summary>
    /// <remarks>
    /// Only SystemAdmin and DepartmentOfficers can create categories for their department.
    /// DepartmentOfficers can only create categories for their own department.
    /// </remarks>
    [HttpPost("{departmentId}/categories")]
    [Authorize(Roles = "SystemAdmin,DepartmentOfficer")]
    public async Task<IActionResult> CreateDepartmentCategory(
        int departmentId,
        [FromBody] CreateDepartmentCategoryRequestDto request)
    {
        // Create the category with the department ID from route
        var createRequest = new CreateCategoryRequestDto
        {
            Name = request.Name,
            Description = request.Description,
            DepartmentId = departmentId
        };

        var category = await _categoryService.CreateCategoryAsync(createRequest);

        return CreatedAtAction(
            nameof(GetDepartmentCategories),
            new { departmentId = departmentId },
            category
        );
    }
}

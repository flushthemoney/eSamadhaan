using eSamadhaan.Application.DTOs.Category;
using eSamadhaan.Application.DTOs.Department;
using eSamadhaan.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eSamadhaan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly ICategoryService _categoryService;
    private readonly IUserService _userService;

    public DepartmentController(
        IDepartmentService departmentService,
        ICategoryService categoryService,
        IUserService userService)
    {
        _departmentService = departmentService;
        _categoryService = categoryService;
        _userService = userService;
    }

    /// <summary>
    /// Create a new department
    /// </summary>
    /// <remarks>
    /// Only SystemAdmin can create departments.
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
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
    [Authorize(Roles = "SystemAdmin")]
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
    /// All authenticated users can view departments.
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
    [Authorize(Roles = "SystemAdmin")]
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
    [Authorize(Roles = "SystemAdmin")]
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
    public async Task<IActionResult> GetDepartmentCategories(int departmentId)
    {
        var categories = await _categoryService.GetCategoriesByDepartmentAsync(departmentId);
        return Ok(categories);
    }

}

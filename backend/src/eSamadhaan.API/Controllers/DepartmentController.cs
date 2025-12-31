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

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
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
}

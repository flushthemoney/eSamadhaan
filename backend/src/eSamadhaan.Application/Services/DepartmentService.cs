using eSamadhaan.Application.DTOs.Department;
using eSamadhaan.Application.Exceptions;
using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Application.Interfaces.Services;
using eSamadhaan.Domain.Entities;

namespace eSamadhaan.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IGrievanceRepository _grievanceRepository;

    public DepartmentService(
        IDepartmentRepository departmentRepository,
        IGrievanceRepository grievanceRepository)
    {
        _departmentRepository = departmentRepository;
        _grievanceRepository = grievanceRepository;
    }

    public async Task<int> CreateDepartmentAsync(string name, string description)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("Department name is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ValidationException("Department description is required.");
        }

        // Create department
        var department = new Department
        {
            Name = name,
            Description = description
        };

        var createdDepartment = await _departmentRepository.CreateAsync(department);
        return createdDepartment.Id;
    }

    public async Task<object?> GetDepartmentByIdAsync(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        
        if (department == null)
        {
            throw new NotFoundException("Department", id);
        }

        return MapToDto(department);
    }

    public async Task<IEnumerable<object>> GetAllDepartmentsAsync()
    {
        var departments = await _departmentRepository.GetAllAsync();
        return departments.Select(MapToDto);
    }

    public async Task UpdateDepartmentAsync(int id, string name, string description)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        
        if (department == null)
        {
            throw new NotFoundException("Department", id);
        }

        // Validate inputs
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("Department name is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ValidationException("Department description is required.");
        }

        department.Name = name;
        department.Description = description;

        await _departmentRepository.UpdateAsync(department);
    }

    public async Task DeleteDepartmentAsync(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        
        if (department == null)
        {
            throw new NotFoundException("Department", id);
        }

        // Check if department has associated grievances
        var grievanceCount = await _grievanceRepository.CountByDepartmentAsync(id);
        if (grievanceCount > 0)
        {
            throw new BusinessRuleViolationException($"Cannot delete department with {grievanceCount} associated grievances.");
        }

        await _departmentRepository.DeleteAsync(id);
    }

    public async Task<bool> DepartmentExistsAsync(int id)
    {
        return await _departmentRepository.ExistsAsync(id);
    }

    // Private helper methods
    private DepartmentDto MapToDto(Department department)
    {
        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description
        };
    }
}

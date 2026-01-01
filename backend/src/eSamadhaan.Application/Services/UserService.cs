using eSamadhaan.Application.DTOs.Auth;
using eSamadhaan.Application.Exceptions;
using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Application.Interfaces.Services;

namespace eSamadhaan.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(
        IUserRepository userRepository,
        IDepartmentRepository departmentRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _departmentRepository = departmentRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        
        if (user == null)
        {
            throw new NotFoundException("User", id);
        }

        return MapToDto(user);
    }

    public async Task<UserDto> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        
        if (user == null)
        {
            throw new NotFoundException($"User with email '{email}' was not found.");
        }

        return MapToDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<IEnumerable<UserDto>> GetUsersByDepartmentAsync(int departmentId)
    {
        // Validate department exists
        if (!await _departmentRepository.ExistsAsync(departmentId))
        {
            throw new NotFoundException("Department", departmentId);
        }

        var users = await _userRepository.GetByDepartmentIdAsync(departmentId);
        return users.Select(MapToDto);
    }

    public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role)
    {
        // Validate role
        var validRoles = new[] { "SystemAdmin", "DepartmentOfficer", "SupervisoryOfficer", "Citizen" };
        if (!validRoles.Contains(role))
        {
            throw new ValidationException($"Invalid role. Valid roles are: {string.Join(", ", validRoles)}");
        }

        var users = await _userRepository.GetByRoleAsync(role);
        return users.Select(MapToDto);
    }

    public async Task<IEnumerable<UserDto>> GetActiveUsersAsync()
    {
        var users = await _userRepository.GetActiveUsersAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequestDto request)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Password is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            throw new ValidationException("Role is required.");
        }

        // Validate role
        var validRoles = new[] { "SystemAdmin", "DepartmentOfficer", "SupervisoryOfficer", "Citizen" };
        if (!validRoles.Contains(request.Role))
        {
            throw new ValidationException($"Invalid role. Valid roles are: {string.Join(", ", validRoles)}");
        }

        // For DepartmentOfficer, department is required
        if (request.Role == "DepartmentOfficer" && !request.DepartmentId.HasValue)
        {
            throw new ValidationException($"{request.Role} must be associated with a department.");
        }

        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email.ToLower()))
        {
            throw new DuplicateException("User", "Email", request.Email);
        }

        // Validate department if provided
        if (request.DepartmentId.HasValue)
        {
            if (!await _departmentRepository.ExistsAsync(request.DepartmentId.Value))
            {
                throw new NotFoundException("Department", request.DepartmentId.Value);
            }
        }

        // Create user entity
        var user = new Domain.Entities.User
        {
            Name = request.Name,
            Email = request.Email.ToLower(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = request.Role,
            DepartmentId = request.DepartmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.CreateAsync(user);
        return MapToDto(createdUser);
    }

    public async Task UpdateUserAsync(int id, string name, string email, string? role, int? departmentId)
    {
        var user = await _userRepository.GetByIdAsync(id);
        
        if (user == null)
        {
            throw new NotFoundException("User", id);
        }

        // Validate inputs
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ValidationException("Email is required.");
        }

        // Check if email is already taken by another user
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null && existingUser.Id != id)
        {
            throw new DuplicateException("User", "Email", email);
        }

        // Validate role if provided
        if (!string.IsNullOrWhiteSpace(role))
        {
            var validRoles = new[] { "SystemAdmin", "DepartmentOfficer", "SupervisoryOfficer", "Citizen" };
            if (!validRoles.Contains(role))
            {
                throw new ValidationException($"Invalid role. Valid roles are: {string.Join(", ", validRoles)}");
            }

            // For DepartmentOfficer, department is required
            if (role == "DepartmentOfficer" && !departmentId.HasValue)
            {
                throw new ValidationException($"{role} must be associated with a department.");
            }

            user.Role = role;
        }

        // Validate department if provided
        if (departmentId.HasValue)
        {
            if (!await _departmentRepository.ExistsAsync(departmentId.Value))
            {
                throw new NotFoundException("Department", departmentId.Value);
            }
        }

        user.Name = name;
        user.Email = email.ToLower();
        user.DepartmentId = departmentId;

        await _userRepository.UpdateAsync(user);
    }

    public async Task UpdateUserStatusAsync(int id, bool isActive)
    {
        var user = await _userRepository.GetByIdAsync(id);
        
        if (user == null)
        {
            throw new NotFoundException("User", id);
        }

        if (user.IsActive == isActive)
        {
            throw new BusinessRuleViolationException($"User is already {(isActive ? "active" : "inactive")}.");
        }

        user.IsActive = isActive;
        await _userRepository.UpdateAsync(user);
    }

    public async Task<Dictionary<string, int>> GetUserCountByRoleAsync()
    {
        var users = await _userRepository.GetAllAsync();
        
        // LINQ aggregation: Count users grouped by role
        var result = users
            .GroupBy(u => u.Role)
            .Select(group => new { Role = group.Key, Count = group.Count() })
            .ToDictionary(x => x.Role, x => x.Count);

        // Ensure all roles are represented
        var validRoles = new[] { "SystemAdmin", "DepartmentOfficer", "SupervisoryOfficer", "Citizen" };
        foreach (var role in validRoles)
        {
            if (!result.ContainsKey(role))
            {
                result[role] = 0;
            }
        }

        return result;
    }

    public async Task<Dictionary<int, int>> GetOfficerCountByDepartmentAsync()
    {
        var users = await _userRepository.GetAllAsync();
        
        // LINQ aggregation: Count DepartmentOfficers grouped by department
        // Note: SupervisoryOfficers are independent and not tied to specific departments
        return users
            .Where(u => u.Role == "DepartmentOfficer" && u.DepartmentId.HasValue)
            .GroupBy(u => u.DepartmentId!.Value)
            .Select(group => new { DepartmentId = group.Key, Count = group.Count() })
            .ToDictionary(x => x.DepartmentId, x => x.Count);
    }

    // Private helper methods
    private UserDto MapToDto(Domain.Entities.User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            DepartmentId = user.DepartmentId,
            DepartmentName = user.Department?.Name ?? string.Empty,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}

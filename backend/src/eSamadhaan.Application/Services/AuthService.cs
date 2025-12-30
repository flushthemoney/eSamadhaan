using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using eSamadhaan.Application.Exceptions;
using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Application.Interfaces.Services;
using eSamadhaan.Domain.Entities;

namespace eSamadhaan.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        
        if (user == null)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("User account is inactive.");
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        return GenerateToken(user.Id, user.Email, user.Role);
    }

    public async Task<int> RegisterUserAsync(string name, string email, string password, string role, int? departmentId)
    {
        // Validate email uniqueness
        if (await _userRepository.EmailExistsAsync(email))
        {
            throw new DuplicateException("User", "Email", email);
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

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            throw new ValidationException("Password must be at least 6 characters long.");
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ValidationException("Role is required.");
        }

        // Validate role
        var validRoles = new[] { "SystemAdmin", "DepartmentOfficer", "SupervisoryOfficer", "Citizen" };
        if (!validRoles.Contains(role))
        {
            throw new ValidationException($"Invalid role. Valid roles are: {string.Join(", ", validRoles)}");
        }

        // For officers, department is required
        if ((role == "DepartmentOfficer" || role == "SupervisoryOfficer") && !departmentId.HasValue)
        {
            throw new ValidationException($"{role} must be associated with a department.");
        }

        var user = new User
        {
            Name = name,
            Email = email.ToLower(),
            PasswordHash = HashPassword(password),
            Role = role,
            DepartmentId = departmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.CreateAsync(user);
        return createdUser.Id;
    }

    public async Task<bool> ValidateUserAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        
        if (user == null || !user.IsActive)
        {
            return false;
        }

        return VerifyPassword(password, user.PasswordHash);
    }

    public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        if (!VerifyPassword(currentPassword, user.PasswordHash))
        {
            throw new UnauthorizedException("Current password is incorrect.");
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            throw new ValidationException("New password must be at least 6 characters long.");
        }

        user.PasswordHash = HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        return !await _userRepository.EmailExistsAsync(email);
    }

    public string GenerateToken(int userId, string email, string role)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured.");
        var issuer = jwtSettings["Issuer"] ?? "eSamadhaan";
        var audience = jwtSettings["Audience"] ?? "eSamadhaan";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string HashPassword(string password)
    {
        using var hmac = new HMACSHA512();
        var salt = hmac.Key;
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        
        // Combine salt and hash
        var hashBytes = new byte[salt.Length + hash.Length];
        Array.Copy(salt, 0, hashBytes, 0, salt.Length);
        Array.Copy(hash, 0, hashBytes, salt.Length, hash.Length);
        
        return Convert.ToBase64String(hashBytes);
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        var hashBytes = Convert.FromBase64String(storedHash);
        
        // Extract salt (first 128 bytes for HMACSHA512)
        var salt = new byte[128];
        Array.Copy(hashBytes, 0, salt, 0, 128);
        
        using var hmac = new HMACSHA512(salt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        
        // Compare computed hash with stored hash
        for (int i = 0; i < computedHash.Length; i++)
        {
            if (hashBytes[i + 128] != computedHash[i])
            {
                return false;
            }
        }
        
        return true;
    }
}

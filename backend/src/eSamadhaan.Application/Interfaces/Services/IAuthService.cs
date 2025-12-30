namespace eSamadhaan.Application.Interfaces.Services;

public interface IAuthService
{
    Task<string> LoginAsync(string email, string password);
    Task<int> RegisterUserAsync(string name, string email, string password, string role, int? departmentId);
    Task<bool> ValidateUserAsync(string email, string password);
    Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<bool> IsEmailAvailableAsync(string email);
    string GenerateToken(int userId, string email, string role);
}

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using eSamadhaan.Application.Exceptions;
using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Application.Interfaces.Services;
using eSamadhaan.Application.Services;
using eSamadhaan.Domain.Entities;

namespace eSamadhaan.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _configurationMock = new Mock<IConfiguration>();
        _passwordHasherMock = new Mock<IPasswordHasher>();

        // Setup default JWT configuration
        var jwtSettingsSection = new Mock<IConfigurationSection>();
        jwtSettingsSection.Setup(x => x["SecretKey"]).Returns("TestSecretKeyThatIsAtLeast32CharactersLong");
        jwtSettingsSection.Setup(x => x["Issuer"]).Returns("eSamadhaan");
        jwtSettingsSection.Setup(x => x["Audience"]).Returns("eSamadhaan");
        jwtSettingsSection.Setup(x => x["ExpiryMinutes"]).Returns("60");

        _configurationMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSettingsSection.Object);

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _configurationMock.Object,
            _passwordHasherMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokenForValidCredentials()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123";
        var user = new User
        {
            Id = 1,
            Email = email,
            PasswordHash = "hashedPassword",
            Role = "Citizen",
            IsActive = true
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.Should().NotBeNullOrEmpty();
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(email), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(password, user.PasswordHash), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedExceptionForInvalidEmail()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var password = "Password123";

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(email, password);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(email), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedExceptionForInvalidPassword()
    {
        // Arrange
        var email = "test@example.com";
        var password = "WrongPassword";
        var user = new User
        {
            Id = 1,
            Email = email,
            PasswordHash = "hashedPassword",
            Role = "Citizen",
            IsActive = true
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(password, user.PasswordHash))
            .Returns(false);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(email, password);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(email), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(password, user.PasswordHash), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedExceptionForInactiveUser()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123";
        var user = new User
        {
            Id = 1,
            Email = email,
            PasswordHash = "hashedPassword",
            Role = "Citizen",
            IsActive = false
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(email, password);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User account is inactive.");
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(email), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldCreateUserForValidInput()
    {
        // Arrange
        var name = "Test User";
        var email = "test@example.com";
        var password = "Password123";
        var role = "Citizen";
        var hashedPassword = "hashedPassword";
        var createdUser = new User
        {
            Id = 1,
            Name = name,
            Email = email.ToLower(),
            PasswordHash = hashedPassword,
            Role = role,
            IsActive = true
        };

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(email))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(x => x.HashPassword(password))
            .Returns(hashedPassword);
        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _authService.RegisterUserAsync(name, email, password, role, null);

        // Assert
        result.Should().Be(createdUser.Id);
        _userRepositoryMock.Verify(x => x.EmailExistsAsync(email), Times.Once);
        _passwordHasherMock.Verify(x => x.HashPassword(password), Times.Once);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldThrowValidationExceptionForInvalidInput()
    {
        // Arrange
        var name = "";
        var email = "test@example.com";
        var password = "Password123";
        var role = "Citizen";

        // Act
        Func<Task> act = async () => await _authService.RegisterUserAsync(name, email, password, role, null);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Name is required.");
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldThrowValidationExceptionForShortPassword()
    {
        // Arrange
        var name = "Test User";
        var email = "test@example.com";
        var password = "12345"; // Less than 6 characters
        var role = "Citizen";

        // Act
        Func<Task> act = async () => await _authService.RegisterUserAsync(name, email, password, role, null);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Password must be at least 6 characters long.");
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldThrowDuplicateExceptionForExistingEmail()
    {
        // Arrange
        var name = "Test User";
        var email = "existing@example.com";
        var password = "Password123";
        var role = "Citizen";

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(email))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _authService.RegisterUserAsync(name, email, password, role, null);

        // Assert
        await act.Should().ThrowAsync<DuplicateException>();
        _userRepositoryMock.Verify(x => x.EmailExistsAsync(email), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldUpdatePasswordForValidInput()
    {
        // Arrange
        var userId = 1;
        var currentPassword = "OldPassword123";
        var newPassword = "NewPassword123";
        var oldHash = "oldHash";
        var newHash = "newHash";
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = oldHash,
            IsActive = true
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(currentPassword, oldHash))
            .Returns(true);
        _passwordHasherMock.Setup(x => x.HashPassword(newPassword))
            .Returns(newHash);

        // Act
        await _authService.ChangePasswordAsync(userId, currentPassword, newPassword);

        // Assert
        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(currentPassword, oldHash), Times.Once);
        _passwordHasherMock.Verify(x => x.HashPassword(newPassword), Times.Once);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u => u.PasswordHash == newHash)), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldThrowExceptionForIncorrectCurrentPassword()
    {
        // Arrange
        var userId = 1;
        var currentPassword = "WrongPassword";
        var newPassword = "NewPassword123";
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "oldHash",
            IsActive = true
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(currentPassword, user.PasswordHash))
            .Returns(false);

        // Act
        Func<Task> act = async () => await _authService.ChangePasswordAsync(userId, currentPassword, newPassword);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Current password is incorrect.");
        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(currentPassword, user.PasswordHash), Times.Once);
        _passwordHasherMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
}


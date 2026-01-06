using FluentAssertions;
using Moq;
using eSamadhaan.Application.DTOs.Auth;
using eSamadhaan.Application.Exceptions;
using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Application.Interfaces.Services;
using eSamadhaan.Application.Services;
using eSamadhaan.Domain.Entities;

namespace eSamadhaan.Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IDepartmentRepository> _departmentRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _departmentRepositoryMock = new Mock<IDepartmentRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();

        _userService = new UserService(
            _userRepositoryMock.Object,
            _departmentRepositoryMock.Object,
            _passwordHasherMock.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUserForValidId()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            Role = "Citizen",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Name.Should().Be(user.Name);
        result.Email.Should().Be(user.Email);
        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldThrowNotFoundExceptionForInvalidId()
    {
        // Arrange
        var userId = 999;

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _userService.GetUserByIdAsync(userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldCreateUserForValidInput()
    {
        // Arrange
        var request = new CreateUserRequestDto
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password123",
            Role = "Citizen"
        };
        var hashedPassword = "hashedPassword";
        var createdUser = new User
        {
            Id = 1,
            Name = request.Name,
            Email = request.Email.ToLower(),
            PasswordHash = hashedPassword,
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email.ToLower()))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(x => x.HashPassword(request.Password))
            .Returns(hashedPassword);
        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(createdUser.Id);
        result.Name.Should().Be(request.Name);
        result.Email.Should().Be(request.Email.ToLower());
        _userRepositoryMock.Verify(x => x.EmailExistsAsync(request.Email.ToLower()), Times.Once);
        _passwordHasherMock.Verify(x => x.HashPassword(request.Password), Times.Once);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldThrowValidationExceptionForInvalidInput()
    {
        // Arrange
        var request = new CreateUserRequestDto
        {
            Name = "", // Empty name
            Email = "test@example.com",
            Password = "Password123",
            Role = "Citizen"
        };

        // Act
        Func<Task> act = async () => await _userService.CreateUserAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Name is required.");
    }

    [Fact]
    public async Task CreateUserAsync_ShouldThrowValidationExceptionForInvalidRole()
    {
        // Arrange
        var request = new CreateUserRequestDto
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password123",
            Role = "InvalidRole"
        };

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email.ToLower()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _userService.CreateUserAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateUserAsync_ShouldThrowDuplicateExceptionForExistingEmail()
    {
        // Arrange
        var request = new CreateUserRequestDto
        {
            Name = "Test User",
            Email = "existing@example.com",
            Password = "Password123",
            Role = "Citizen"
        };

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email.ToLower()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _userService.CreateUserAsync(request);

        // Assert
        await act.Should().ThrowAsync<DuplicateException>();
        _userRepositoryMock.Verify(x => x.EmailExistsAsync(request.Email.ToLower()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserStatusAsync_ShouldUpdateStatus()
    {
        // Arrange
        var userId = 1;
        var isActive = false;
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            IsActive = true // Currently active
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        await _userService.UpdateUserStatusAsync(userId, isActive);

        // Assert
        user.IsActive.Should().Be(isActive);
        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u => u.IsActive == isActive)), Times.Once);
    }

    [Fact]
    public async Task UpdateUserStatusAsync_ShouldThrowNotFoundExceptionForInvalidId()
    {
        // Arrange
        var userId = 999;
        var isActive = false;

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _userService.UpdateUserStatusAsync(userId, isActive);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserStatusAsync_ShouldThrowBusinessRuleViolationExceptionWhenStatusIsSame()
    {
        // Arrange
        var userId = 1;
        var isActive = true;
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            IsActive = true // Already active
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _userService.UpdateUserStatusAsync(userId, isActive);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage("User is already active.");
        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
}


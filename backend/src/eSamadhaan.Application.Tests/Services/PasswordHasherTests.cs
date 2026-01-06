using FluentAssertions;
using eSamadhaan.Application.Services;

namespace eSamadhaan.Application.Tests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_ShouldReturnDifferentHashForSamePassword()
    {
        // Arrange
        var password = "TestPassword123";

        // Act
        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        // Assert
        hash1.Should().NotBeNullOrEmpty();
        hash2.Should().NotBeNullOrEmpty();
        // Hashes should be different due to random salt
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
    {
        // Arrange
        var password = "TestPassword123";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
    {
        // Arrange
        var correctPassword = "TestPassword123";
        var incorrectPassword = "WrongPassword";
        var hash = _passwordHasher.HashPassword(correctPassword);

        // Act
        var result = _passwordHasher.VerifyPassword(incorrectPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_ShouldProduceValidBase64String()
    {
        // Arrange
        var password = "TestPassword123";

        // Act
        var hash = _passwordHasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        // Verify it's valid Base64
        Action act = () => Convert.FromBase64String(hash);
        act.Should().NotThrow();
    }
}


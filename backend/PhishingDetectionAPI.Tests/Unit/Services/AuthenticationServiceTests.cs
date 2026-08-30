namespace PhishingDetectionAPI.Tests.Unit.Services;

using Moq;
using PhishingDetectionAPI.Application.Dtos;
using PhishingDetectionAPI.Application.Services;
using PhishingDetectionAPI.Domain.Entities;
using PhishingDetectionAPI.Domain.Interfaces;
using PhishingDetectionAPI.Infrastructure.Security;
using Xunit;

public class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly JwtSettings _jwtSettings = new()
    {
        SecretKey = "unit-test-secret-key-with-at-least-32-chars",
        ExpirationMinutes = 60,
        Issuer = "PhishingDetectionAPI.Tests",
        Audience = "PhishingDetectionAPI.Tests.Clients"
    };

    private AuthenticationService CreateService() => new(_userRepositoryMock.Object, _jwtSettings);

    private static User CreateActiveUser(string username = "john") => new()
    {
        Id = 1,
        Username = username,
        Email = $"{username}@empresa.com",
        PasswordHash = "some-hash",
        FullName = "John Doe",
        Role = UserRole.User,
        IsActive = true
    };

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ReturnsTokenAndUser()
    {
        var user = CreateActiveUser();
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync(user.Username)).ReturnsAsync(user);
        var service = CreateService();

        var result = await service.AuthenticateAsync(new LoginRequest { Username = user.Username, Password = "secret" });

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(user.Username, result.User?.Username);
    }

    [Fact]
    public async Task AuthenticateAsync_WithNonExistentUser_ThrowsInvalidOperationException()
    {
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AuthenticateAsync(new LoginRequest { Username = "ghost", Password = "secret" }));
    }

    [Fact]
    public async Task AuthenticateAsync_WithInactiveUser_ThrowsInvalidOperationException()
    {
        var user = CreateActiveUser();
        user.IsActive = false;
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync(user.Username)).ReturnsAsync(user);
        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AuthenticateAsync(new LoginRequest { Username = user.Username, Password = "secret" }));
    }

    [Theory]
    [InlineData("", "secret")]
    [InlineData("john", "")]
    public async Task AuthenticateAsync_WithMissingCredentials_ThrowsInvalidOperationException(string username, string password)
    {
        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AuthenticateAsync(new LoginRequest { Username = username, Password = password }));
    }

    [Fact]
    public async Task ValidateToken_WithTokenIssuedByAuthenticate_ReturnsTrue()
    {
        var user = CreateActiveUser();
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync(user.Username)).ReturnsAsync(user);
        var service = CreateService();
        var authResponse = await service.AuthenticateAsync(new LoginRequest { Username = user.Username, Password = "secret" });

        var isValid = service.ValidateToken(authResponse.Token);

        Assert.True(isValid);
    }

    [Fact]
    public void ValidateToken_WithGarbageToken_ReturnsFalse()
    {
        var service = CreateService();

        var isValid = service.ValidateToken("not-a-real-jwt-token");

        Assert.False(isValid);
    }

    [Fact]
    public async Task GetUserIdFromToken_ReturnsIdUsedToAuthenticate()
    {
        var user = CreateActiveUser();
        _userRepositoryMock.Setup(r => r.GetByUsernameAsync(user.Username)).ReturnsAsync(user);
        var service = CreateService();
        var authResponse = await service.AuthenticateAsync(new LoginRequest { Username = user.Username, Password = "secret" });

        var userId = service.GetUserIdFromToken(authResponse.Token);

        Assert.Equal(user.Id, userId);
    }
}

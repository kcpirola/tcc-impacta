namespace PhishingDetectionAPI.Tests.Unit.Services;

using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PhishingDetectionAPI.Application.Mapping;
using PhishingDetectionAPI.Application.Services;
using PhishingDetectionAPI.Domain.Entities;
using PhishingDetectionAPI.Domain.Interfaces;
using Xunit;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly IMapper _mapper;

    public UserServiceTests()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        _mapper = configuration.CreateMapper();
    }

    private UserService CreateService() => new(_userRepositoryMock.Object, _mapper);

    [Fact]
    public async Task RegisterUserAsync_WithNewCredentials_CreatesAndReturnsUser()
    {
        _userRepositoryMock.Setup(r => r.UsernameExistsAsync("newuser")).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.EmailExistsAsync("newuser@empresa.com")).ReturnsAsync(false);
        var service = CreateService();

        var result = await service.RegisterUserAsync("newuser", "newuser@empresa.com", "secret", "New User");

        Assert.Equal("newuser", result.Username);
        Assert.Equal("New User", result.FullName);
        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Username == "newuser")), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_StoresBcryptHashAndAppliesRoleAndDepartment()
    {
        _userRepositoryMock.Setup(r => r.UsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        User? gravado = null;
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).Callback<User>(u => gravado = u);
        var service = CreateService();

        await service.RegisterUserAsync("ana", "ana@empresa.com", "senha-forte-123", "Ana", "Financeiro", UserRole.Analyst);

        Assert.NotNull(gravado);
        Assert.NotEqual("senha-forte-123", gravado!.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("senha-forte-123", gravado.PasswordHash));
        Assert.Equal("Financeiro", gravado.Department);
        Assert.Equal(UserRole.Analyst, gravado.Role);
    }

    [Fact]
    public async Task RegisterUserAsync_WithExistingUsername_ThrowsInvalidOperationException()
    {
        _userRepositoryMock.Setup(r => r.UsernameExistsAsync("existing")).ReturnsAsync(true);
        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RegisterUserAsync("existing", "existing@empresa.com", "secret", "Existing User"));
    }

    [Fact]
    public async Task RegisterUserAsync_WithExistingEmail_ThrowsInvalidOperationException()
    {
        _userRepositoryMock.Setup(r => r.UsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.EmailExistsAsync("dup@empresa.com")).ReturnsAsync(true);
        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RegisterUserAsync("newuser", "dup@empresa.com", "secret", "New User"));
    }

    [Fact]
    public async Task GetUserAsync_WithExistingId_ReturnsUserDto()
    {
        var user = new User { Id = 5, Username = "john", Email = "john@empresa.com", FullName = "John" };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);
        var service = CreateService();

        var result = await service.GetUserAsync(5);

        Assert.NotNull(result);
        Assert.Equal("john", result!.Username);
    }

    [Fact]
    public async Task GetUserAsync_WithNonExistingId_ReturnsNull()
    {
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User?)null);
        var service = CreateService();

        var result = await service.GetUserAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeactivateUserAsync_SetsIsActiveToFalse()
    {
        var user = new User { Id = 1, Username = "john", IsActive = true };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        var service = CreateService();

        await service.DeactivateUserAsync(1);

        Assert.False(user.IsActive);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u => !u.IsActive)), Times.Once);
    }

    [Fact]
    public async Task DeactivateUserAsync_WithNonExistingId_ThrowsInvalidOperationException()
    {
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User?)null);
        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeactivateUserAsync(999));
    }
}

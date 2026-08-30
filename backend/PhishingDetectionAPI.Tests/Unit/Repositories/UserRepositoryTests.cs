namespace PhishingDetectionAPI.Tests.Unit.Repositories;

using Microsoft.EntityFrameworkCore;
using PhishingDetectionAPI.Domain.Entities;
using PhishingDetectionAPI.Infrastructure.Data;
using PhishingDetectionAPI.Infrastructure.Data.Repositories;
using Xunit;

public class UserRepositoryTests
{
    private static PhishingDetectionContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PhishingDetectionContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PhishingDetectionContext(options);
    }

    [Fact]
    public async Task GetByUsernameAsync_ReturnsMatchingUser()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context);
        await repository.AddAsync(new User { Username = "john", Email = "john@empresa.com", PasswordHash = "hash", FullName = "John" });

        var result = await repository.GetByUsernameAsync("john");

        Assert.NotNull(result);
        Assert.Equal("john@empresa.com", result!.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_WithUnknownEmail_ReturnsNull()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context);

        var result = await repository.GetByEmailAsync("ghost@empresa.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task UsernameExistsAsync_ReturnsTrueWhenUsernameIsTaken()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context);
        await repository.AddAsync(new User { Username = "john", Email = "john@empresa.com", PasswordHash = "hash", FullName = "John" });

        Assert.True(await repository.UsernameExistsAsync("john"));
        Assert.False(await repository.UsernameExistsAsync("jane"));
    }

    [Fact]
    public async Task EmailExistsAsync_ReturnsTrueWhenEmailIsTaken()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context);
        await repository.AddAsync(new User { Username = "john", Email = "john@empresa.com", PasswordHash = "hash", FullName = "John" });

        Assert.True(await repository.EmailExistsAsync("john@empresa.com"));
        Assert.False(await repository.EmailExistsAsync("jane@empresa.com"));
    }
}

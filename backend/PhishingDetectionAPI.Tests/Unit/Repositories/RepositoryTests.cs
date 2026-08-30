namespace PhishingDetectionAPI.Tests.Unit.Repositories;

using Microsoft.EntityFrameworkCore;
using PhishingDetectionAPI.Domain.Entities;
using PhishingDetectionAPI.Infrastructure.Data;
using PhishingDetectionAPI.Infrastructure.Data.Repositories;
using Xunit;

public class RepositoryTests
{
    private static PhishingDetectionContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PhishingDetectionContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PhishingDetectionContext(options);
    }

    [Fact]
    public async Task AddAsync_PersistsEntity()
    {
        await using var context = CreateContext();
        var repository = new Repository<User>(context);
        var user = new User { Username = "john", Email = "john@empresa.com", PasswordHash = "hash", FullName = "John" };

        await repository.AddAsync(user);

        Assert.True(user.Id > 0);
        Assert.Equal(1, await context.Users.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPersistedEntity()
    {
        await using var context = CreateContext();
        var repository = new Repository<User>(context);
        var user = new User { Username = "john", Email = "john@empresa.com", PasswordHash = "hash", FullName = "John" };
        await repository.AddAsync(user);

        var result = await repository.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal("john", result!.Username);
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
    {
        await using var context = CreateContext();
        var repository = new Repository<User>(context);

        var result = await repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChangesAndRefreshesTimestamp()
    {
        await using var context = CreateContext();
        var repository = new Repository<User>(context);
        var user = new User { Username = "john", Email = "john@empresa.com", PasswordHash = "hash", FullName = "John" };
        await repository.AddAsync(user);
        var originalUpdatedAt = user.UpdatedAt;

        user.FullName = "John Updated";
        await repository.UpdateAsync(user);

        var result = await repository.GetByIdAsync(user.Id);
        Assert.Equal("John Updated", result!.FullName);
        Assert.True(result.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public async Task RemoveAsync_ById_DeletesEntity()
    {
        await using var context = CreateContext();
        var repository = new Repository<User>(context);
        var user = new User { Username = "john", Email = "john@empresa.com", PasswordHash = "hash", FullName = "John" };
        await repository.AddAsync(user);

        await repository.RemoveAsync(user.Id);

        Assert.Equal(0, await context.Users.CountAsync());
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrueOnlyForPersistedEntity()
    {
        await using var context = CreateContext();
        var repository = new Repository<User>(context);
        var user = new User { Username = "john", Email = "john@empresa.com", PasswordHash = "hash", FullName = "John" };
        await repository.AddAsync(user);

        Assert.True(await repository.ExistsAsync(user.Id));
        Assert.False(await repository.ExistsAsync(999));
    }
}

namespace PhishingDetectionAPI.Tests.Integration;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PhishingDetectionAPI.Domain.Entities;
using PhishingDetectionAPI.Infrastructure.Data;
using PhishingDetectionAPI.Infrastructure.MachineLearning;

/// <summary>
/// Sobe a aplicação real (Program.cs) em memória, substituindo o PostgreSQL por um
/// banco InMemory e o microserviço de ML por um stub, para testes de integração isolados.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestUsername = "testuser";
    public const string TestPassword = "any-password";

    // WebApplicationFactory pode invocar ConfigureWebHost mais de uma vez; um nome fixo
    // garante que todas as chamadas apontem para o mesmo banco InMemory.
    private readonly string _dbName = $"IntegrationTestsDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<PhishingDetectionContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.RemoveAll<IDbContextOptionsConfiguration<PhishingDetectionContext>>();

            services.AddDbContext<PhishingDetectionContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            var mlClientDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMlServiceClient));
            if (mlClientDescriptor != null)
            {
                services.Remove(mlClientDescriptor);
            }
            services.AddScoped<IMlServiceClient, FakeMlServiceClient>();

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PhishingDetectionContext>();
            db.Database.EnsureCreated();
            SeedTestUser(db);
        });
    }

    private static void SeedTestUser(PhishingDetectionContext db)
    {
        if (db.Users.Any(u => u.Username == TestUsername))
        {
            return;
        }

        db.Users.Add(new User
        {
            Username = TestUsername,
            Email = "testuser@empresa.com",
            PasswordHash = "seed-hash",
            FullName = "Test User",
            Role = UserRole.User,
            IsActive = true
        });
        db.SaveChanges();
    }
}

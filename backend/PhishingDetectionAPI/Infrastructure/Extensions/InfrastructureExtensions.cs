namespace PhishingDetectionAPI.Infrastructure.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhishingDetectionAPI.Infrastructure.Data;
using PhishingDetectionAPI.Infrastructure.Data.Repositories;
using PhishingDetectionAPI.Domain.Interfaces;

/// <summary>
/// Extensões para registro de serviços de infraestrutura.
/// Implementa o padrão Extension Methods para facilitar configuração.
/// Segue o princípio de Inversão de Dependência (DIP) do SOLID.
/// </summary>
public static class InfrastructureExtensions
{
    /// <summary>
    /// Registra os serviços de banco de dados e repositórios.
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    /// <param name="configuration">Configuração da aplicação.</param>
    /// <returns>Serviços para encadeamento de chamadas.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Registrar DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<PhishingDetectionContext>(options =>
            options.UseNpgsql(connectionString,
                builder => builder.MigrationsAssembly("PhishingDetectionAPI"))
                .UseSnakeCaseNamingConvention());

        // Registrar Repositórios
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}

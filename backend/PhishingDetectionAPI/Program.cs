using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Refit;
using Serilog;
using System.Text;
using PhishingDetectionAPI.Infrastructure.Extensions;
using PhishingDetectionAPI.Infrastructure.MachineLearning;
using PhishingDetectionAPI.Infrastructure.Security;
using PhishingDetectionAPI.Domain.Interfaces;
using PhishingDetectionAPI.Application.Services;

// Configurar Serilog para logging estruturado
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Adicionar Logging com Serilog
    builder.Host.UseSerilog();

    // Adicionar serviços de infraestrutura (BD, repositórios)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Obter configurações de JWT
    var jwtSettings = new JwtSettings();
    builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);

    if (!jwtSettings.IsValid())
    {
        throw new InvalidOperationException("Configurações de JWT inválidas. Verifique o appsettings.json");
    }

    // Disponibiliza JwtSettings via DI para os serviços que dependem dele (ex.: AuthenticationService)
    builder.Services.AddSingleton(jwtSettings);

    // Adicionar autenticação JWT
    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    // Adicionar Autorização
    builder.Services.AddAuthorization();

    // Registrar serviços de aplicação
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IAuditService, AuditService>();
    builder.Services.AddScoped<IEmailIntegrationService, EmailIntegrationService>();

    // Registrar cliente Refit para o microserviço Python de ML
    var mlServiceBaseUrl = builder.Configuration["MlServiceSettings:BaseUrl"]
        ?? throw new InvalidOperationException("MlServiceSettings:BaseUrl não configurado");
    builder.Services
        .AddRefitClient<IMlServiceClient>()
        .ConfigureHttpClient(c =>
        {
            c.BaseAddress = new Uri(mlServiceBaseUrl);
            c.Timeout = TimeSpan.FromSeconds(
                builder.Configuration.GetValue<double>("MlServiceSettings:Timeout", 30));
        });
    builder.Services.AddScoped<IEmailClassificationService, EmailClassificationService>();

    // Adicionar AutoMapper
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    // Adicionar Controllers
    builder.Services.AddControllers();

    // Adicionar CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    // Adicionar Swagger/OpenAPI
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Phishing Detection API",
            Version = "v1.0",
            Description = "API para detecção de e-mails de phishing usando ML",
            Contact = new OpenApiContact
            {
                Name = "Projeto TCC Impacta",
                Email = "tcc@impacta.edu.br"
            }
        });

        // Adicionar suporte a JWT na documentação Swagger
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        });

        // Incluir comentários XML na documentação
        var xmlFile = $"{AppDomain.CurrentDomain.BaseDirectory}PhishingDetectionAPI.xml";
        if (File.Exists(xmlFile))
        {
            options.IncludeXmlComments(xmlFile);
        }
    });

    var app = builder.Build();

    // Pipeline de middleware HTTP
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Phishing Detection API v1");
            options.RoutePrefix = string.Empty; // Swagger na raiz
        });
    }

    // Redirecionar HTTP para HTTPS em produção
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    // Middleware de CORS
    app.UseCors("AllowAll");

    // Middleware de autenticação e autorização
    app.UseAuthentication();
    app.UseAuthorization();

    // Mapear controllers
    app.MapControllers();

    // Health check básico
    app.MapGet("/health", async Task<IResult> (PhishingDetectionAPI.Infrastructure.Data.PhishingDetectionContext context) =>
    {
        try
        {
            await context.Database.CanConnectAsync();
            return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return Results.Json(new { status = "unhealthy", error = ex.Message }, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    });

    // Iniciar aplicação
    Log.Information("Iniciando Phishing Detection API...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Erro fatal ao executar a aplicação");
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Torna a classe Program acessível ao WebApplicationFactory usado nos testes de integração.
public partial class Program { }


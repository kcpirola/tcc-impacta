namespace PhishingDetectionAPI.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PhishingDetectionAPI.Domain.Entities;

/// <summary>
/// Contexto do Entity Framework Core para o sistema de detecção de phishing.
/// Configura o mapeamento das entidades com o banco de dados PostgreSQL.
/// Implementa o padrão Context do EF Core.
/// </summary>
public class PhishingDetectionContext : DbContext
{
    /// <summary>Inicializa uma nova instância de PhishingDetectionContext.</summary>
    /// <param name="options">Opções de configuração do contexto.</param>
    public PhishingDetectionContext(DbContextOptions<PhishingDetectionContext> options) 
        : base(options)
    {
    }

    // DbSets - Representa as tabelas do banco de dados
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Email> Emails { get; set; } = null!;
    public DbSet<PhishingIndicator> PhishingIndicators { get; set; } = null!;
    public DbSet<UrlDetected> UrlsDetected { get; set; } = null!;
    public DbSet<Attachment> Attachments { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<LgpdConsent> LgpdConsents { get; set; } = null!;
    public DbSet<DataDeletionRequest> DataDeletionRequests { get; set; } = null!;
    public DbSet<SystemConfig> SystemConfigs { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<ModelPerformance> ModelPerformances { get; set; } = null!;
    public DbSet<DailyStatistics> DailyStatistics { get; set; } = null!;

    /// <summary>Configura o modelo de dados e o mapeamento com o banco.</summary>
    /// <param name="modelBuilder">Object para construir o modelo.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Department)
                .HasMaxLength(100);

            entity.Property(e => e.Role)
                .HasConversion(TextoMinusculo<UserRole>())
                .HasMaxLength(50);

            entity.Ignore(e => e.HasLgpdConsent);

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configurar Email
        modelBuilder.Entity<Email>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.MessageId)
                .HasMaxLength(255);

            entity.Property(e => e.Subject)
                .HasMaxLength(500);

            entity.Property(e => e.Sender)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Recipients)
                .HasMaxLength(1000);

            entity.Property(e => e.EmailBody)
                .HasColumnType("text");

            entity.Property(e => e.EmailHeaders)
                .HasColumnType("text");

            entity.Property(e => e.Classification)
                .HasConversion(TextoMinusculo<EmailClassification>())
                .HasMaxLength(50);

            entity.Property(e => e.Confidence)
                .HasPrecision(5, 2);

            entity.Property(e => e.UserFeedback)
                .HasConversion(TextoMinusculo<UserFeedback>())
                .HasMaxLength(50);

            entity.HasIndex(e => e.MessageId).IsUnique();
            entity.HasIndex(e => e.Classification);
            entity.HasIndex(e => e.Sender);
            entity.HasIndex(e => e.ReceivedDate);
            entity.HasIndex(e => e.ProcessedDate);

            // Relacionamentos
            entity.HasMany(e => e.Indicators)
                .WithOne(pi => pi.Email)
                .HasForeignKey(pi => pi.EmailId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.UrlsDetected)
                .WithOne(u => u.Email)
                .HasForeignKey(u => u.EmailId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Attachments)
                .WithOne(a => a.Email)
                .HasForeignKey(a => a.EmailId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurar PhishingIndicator
        modelBuilder.Entity<PhishingIndicator>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.IndicatorType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.IndicatorValue)
                .HasMaxLength(500);

            entity.Property(e => e.RiskScore)
                .HasPrecision(5, 2);

            entity.HasIndex(e => e.EmailId);
        });

        // Configurar UrlDetected
        modelBuilder.Entity<UrlDetected>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(e => e.ReputationScore)
                .HasPrecision(5, 2);

            entity.HasIndex(e => e.EmailId);
        });

        // Configurar Attachment
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.FileHash)
                .HasMaxLength(255);

            entity.Property(e => e.ScanResult)
                .HasColumnType("text");

            entity.HasIndex(e => e.EmailId);
        });

        // Configurar AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ResourceType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasColumnType("text");

            entity.Property(e => e.IpAddress)
                .IsRequired()
                .HasMaxLength(45);

            // audit_logs no schema.sql tem só a coluna "timestamp", sem updated_at nem user_agent.
            entity.Property(e => e.CreatedAt).HasColumnName("timestamp");
            entity.Ignore(e => e.UpdatedAt);
            entity.Ignore(e => e.UserAgent);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.CreatedAt);

            // Relacionamento
            entity.HasOne(e => e.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configurar LgpdConsent
        modelBuilder.Entity<LgpdConsent>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ConsentType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(45);

            entity.Property(e => e.UserAgent)
                .HasColumnType("text");

            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.LgpdConsents)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurar DataDeletionRequest
        modelBuilder.Entity<DataDeletionRequest>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmailAddress)
                .HasMaxLength(255);

            entity.Property(e => e.Reason)
                .HasColumnType("text");

            entity.Property(e => e.Status)
                .HasConversion(TextoMinusculo<DataDeletionStatus>())
                .HasMaxLength(50);

            entity.Property(e => e.Notes)
                .HasColumnType("text");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configurar SystemConfig
        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.ToTable("system_config");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ConfigKey)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ConfigValue)
                .HasColumnType("text");

            entity.Property(e => e.DataType)
                .HasMaxLength(50);

            entity.HasIndex(e => e.ConfigKey).IsUnique();
        });

        // Configurar Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.NotificationType)
                .HasMaxLength(50);

            entity.Property(e => e.Title)
                .HasMaxLength(200);

            entity.Property(e => e.Message)
                .HasColumnType("text");

            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Email)
                .WithMany()
                .HasForeignKey(e => e.EmailId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configurar ModelPerformance
        modelBuilder.Entity<ModelPerformance>(entity =>
        {
            entity.ToTable("model_performance");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ModelName)
                .HasMaxLength(100);

            entity.Property(e => e.ModelVersion)
                .HasMaxLength(20);

            entity.Property(e => e.Accuracy).HasPrecision(5, 4);
            entity.Property(e => e.Precision).HasPrecision(5, 4);
            entity.Property(e => e.Recall).HasPrecision(5, 4);
            entity.Property(e => e.F1Score).HasPrecision(5, 4).HasColumnName("f1_score");
            entity.Property(e => e.Notes).HasColumnType("text");
        });

        // Configurar DailyStatistics
        modelBuilder.Entity<DailyStatistics>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.AvgProcessingTimeMs).HasPrecision(10, 2);
            entity.Property(e => e.ModelAccuracy).HasPrecision(5, 4);

            entity.HasIndex(e => e.StatisticDate).IsUnique();
        });

        // O schema.sql não tem as datas herdadas de BaseEntity nestas tabelas;
        // cada uma já guarda a sua própria data (checked_at, requested_at, tested_at...).
        foreach (var tipo in new[]
        {
            typeof(PhishingIndicator), typeof(UrlDetected), typeof(Attachment), typeof(LgpdConsent),
            typeof(DataDeletionRequest), typeof(Notification), typeof(ModelPerformance), typeof(DailyStatistics)
        })
        {
            modelBuilder.Entity(tipo).Ignore(nameof(BaseEntity.UpdatedAt));
        }

        foreach (var tipo in new[] { typeof(UrlDetected), typeof(DataDeletionRequest), typeof(SystemConfig) })
        {
            modelBuilder.Entity(tipo).Ignore(nameof(BaseEntity.CreatedAt));
        }
    }

    /// <summary>Grava enums como texto minúsculo, o formato usado pelo schema.sql ('safe', 'pending'...).</summary>
    private static ValueConverter<T, string> TextoMinusculo<T>() where T : struct, Enum =>
        new(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<T>(v, true));
}

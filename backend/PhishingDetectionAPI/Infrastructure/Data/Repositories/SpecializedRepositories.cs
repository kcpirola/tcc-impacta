namespace PhishingDetectionAPI.Infrastructure.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using PhishingDetectionAPI.Domain.Entities;
using PhishingDetectionAPI.Domain.Interfaces;

/// <summary>
/// Implementação do repositório específico para User.
/// Herda de Repository e implementa IUserRepository.
/// Adiciona métodos específicos para operações com usuários.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    /// <summary>Inicializa uma nova instância de UserRepository.</summary>
    /// <param name="context">Contexto do banco de dados.</param>
    public UserRepository(PhishingDetectionContext context) : base(context) { }

    /// <summary>Obtém um usuário pelo nome de usuário.</summary>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    /// <summary>Obtém um usuário pelo e-mail.</summary>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>Verifica se um nome de usuário já existe.</summary>
    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbSet.AnyAsync(u => u.Username == username);
    }

    /// <summary>Verifica se um e-mail já está registrado.</summary>
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }
}

/// <summary>
/// Implementação do repositório específico para Email.
/// Herda de Repository e implementa IEmailRepository.
/// Adiciona métodos específicos para operações com e-mails.
/// </summary>
public class EmailRepository : Repository<Email>, IEmailRepository
{
    /// <summary>Inicializa uma nova instância de EmailRepository.</summary>
    /// <param name="context">Contexto do banco de dados.</param>
    public EmailRepository(PhishingDetectionContext context) : base(context) { }

    /// <summary>Obtém e-mails por classificação com paginação.</summary>
    public async Task<IEnumerable<Email>> GetByClassificationAsync(
        EmailClassification classification, int skip = 0, int take = 10)
    {
        return await _dbSet
            .Where(e => e.Classification == classification)
            .Skip(skip)
            .Take(take)
            .OrderByDescending(e => e.ProcessedDate)
            .ToListAsync();
    }

    /// <summary>Obtém e-mails por remetente com paginação.</summary>
    public async Task<IEnumerable<Email>> GetBySenderAsync(
        string sender, int skip = 0, int take = 10)
    {
        return await _dbSet
            .Where(e => e.Sender == sender)
            .Skip(skip)
            .Take(take)
            .OrderByDescending(e => e.ProcessedDate)
            .ToListAsync();
    }

    /// <summary>Obtém um e-mail pelo MessageId.</summary>
    public async Task<Email?> GetByMessageIdAsync(string messageId)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.MessageId == messageId);
    }

    /// <summary>Obtém e-mails não validados pelo usuário com paginação.</summary>
    public async Task<IEnumerable<Email>> GetUnvalidatedEmailsAsync(int skip = 0, int take = 10)
    {
        return await _dbSet
            .Where(e => !e.IsValidated)
            .Skip(skip)
            .Take(take)
            .OrderByDescending(e => e.ProcessedDate)
            .ToListAsync();
    }

    /// <summary>Conta o total de e-mails em uma classificação.</summary>
    public async Task<int> CountByClassificationAsync(EmailClassification classification)
    {
        return await _dbSet.CountAsync(e => e.Classification == classification);
    }
}

/// <summary>
/// Implementação do repositório específico para AuditLog.
/// </summary>
public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    /// <summary>Inicializa uma nova instância de AuditLogRepository.</summary>
    /// <param name="context">Contexto do banco de dados.</param>
    public AuditLogRepository(PhishingDetectionContext context) : base(context) { }

    /// <summary>Obtém logs de auditoria de um usuário específico com paginação.</summary>
    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId, int skip = 0, int take = 10)
    {
        return await _dbSet
            .Where(al => al.UserId == userId)
            .Skip(skip)
            .Take(take)
            .OrderByDescending(al => al.CreatedAt)
            .ToListAsync();
    }

    /// <summary>Obtém logs de auditoria por tipo de ação com paginação.</summary>
    public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action, int skip = 0, int take = 10)
    {
        return await _dbSet
            .Where(al => al.Action == action)
            .Skip(skip)
            .Take(take)
            .OrderByDescending(al => al.CreatedAt)
            .ToListAsync();
    }
}

namespace PhishingDetectionAPI.Domain.Interfaces;

using PhishingDetectionAPI.Domain.Entities;

/// <summary>
/// Interface genérica para repositório.
/// Implementa o padrão Repository e o princípio de segregação de interface do SOLID.
/// </summary>
/// <typeparam name="TEntity">Tipo de entidade gerenciada pelo repositório.</typeparam>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>Obtém uma entidade pelo ID.</summary>
    /// <param name="id">ID da entidade.</param>
    /// <returns>A entidade ou null se não encontrada.</returns>
    Task<TEntity?> GetByIdAsync(int id);

    /// <summary>Obtém todas as entidades.</summary>
    /// <returns>Coleção de todas as entidades.</returns>
    Task<IEnumerable<TEntity>> GetAllAsync();

    /// <summary>Adiciona uma nova entidade.</summary>
    /// <param name="entity">Entidade a adicionar.</param>
    Task AddAsync(TEntity entity);

    /// <summary>Atualiza uma entidade existente.</summary>
    /// <param name="entity">Entidade com dados atualizados.</param>
    Task UpdateAsync(TEntity entity);

    /// <summary>Remove uma entidade.</summary>
    /// <param name="entity">Entidade a remover.</param>
    Task RemoveAsync(TEntity entity);

    /// <summary>Remove uma entidade pelo ID.</summary>
    /// <param name="id">ID da entidade a remover.</param>
    Task RemoveAsync(int id);

    /// <summary>Verifica se uma entidade existe.</summary>
    /// <param name="id">ID da entidade.</param>
    /// <returns>True se existe, False caso contrário.</returns>
    Task<bool> ExistsAsync(int id);
}

/// <summary>
/// Interface específica para repositório de usuários.
/// Herda de IRepository e adiciona operações específicas.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>Obtém um usuário pelo nome de usuário.</summary>
    /// <param name="username">Nome de usuário.</param>
    /// <returns>O usuário ou null se não encontrado.</returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>Obtém um usuário pelo e-mail.</summary>
    /// <param name="email">E-mail do usuário.</param>
    /// <returns>O usuário ou null se não encontrado.</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>Verifica se um nome de usuário já existe.</summary>
    /// <param name="username">Nome de usuário a verificar.</param>
    /// <returns>True se existe, False caso contrário.</returns>
    Task<bool> UsernameExistsAsync(string username);

    /// <summary>Verifica se um e-mail já está registrado.</summary>
    /// <param name="email">E-mail a verificar.</param>
    /// <returns>True se existe, False caso contrário.</returns>
    Task<bool> EmailExistsAsync(string email);
}

/// <summary>
/// Interface específica para repositório de e-mails.
/// </summary>
public interface IEmailRepository : IRepository<Email>
{
    /// <summary>Obtém e-mails por classificação.</summary>
    /// <param name="classification">Classificação do e-mail.</param>
    /// <param name="skip">Número de registros a pular (paginação).</param>
    /// <param name="take">Número de registros a retornar (paginação).</param>
    /// <returns>Coleção de e-mails com a classificação especificada.</returns>
    Task<IEnumerable<Email>> GetByClassificationAsync(EmailClassification classification, int skip = 0, int take = 10);

    /// <summary>Obtém e-mails por remetente.</summary>
    /// <param name="sender">Endereço do remetente.</param>
    /// <param name="skip">Número de registros a pular.</param>
    /// <param name="take">Número de registros a retornar.</param>
    /// <returns>Coleção de e-mails do remetente especificado.</returns>
    Task<IEnumerable<Email>> GetBySenderAsync(string sender, int skip = 0, int take = 10);

    /// <summary>Obtém um e-mail pelo MessageId.</summary>
    /// <param name="messageId">ID única da mensagem.</param>
    /// <returns>O e-mail ou null se não encontrado.</returns>
    Task<Email?> GetByMessageIdAsync(string messageId);

    /// <summary>Obtém e-mails não validados pelo usuário.</summary>
    /// <param name="skip">Número de registros a pular.</param>
    /// <param name="take">Número de registros a retornar.</param>
    /// <returns>Coleção de e-mails não validados.</returns>
    Task<IEnumerable<Email>> GetUnvalidatedEmailsAsync(int skip = 0, int take = 10);

    /// <summary>Conta o total de e-mails em uma classificação.</summary>
    /// <param name="classification">Classificação a contar.</param>
    /// <returns>Número de e-mails na classificação.</returns>
    Task<int> CountByClassificationAsync(EmailClassification classification);
}

/// <summary>
/// Interface para o repositório de logs de auditoria.
/// </summary>
public interface IAuditLogRepository : IRepository<AuditLog>
{
    /// <summary>Obtém logs de auditoria de um usuário específico.</summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="skip">Número de registros a pular.</param>
    /// <param name="take">Número de registros a retornar.</param>
    /// <returns>Coleção de logs de auditoria do usuário.</returns>
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId, int skip = 0, int take = 10);

    /// <summary>Obtém logs de auditoria por tipo de ação.</summary>
    /// <param name="action">Tipo de ação.</param>
    /// <param name="skip">Número de registros a pular.</param>
    /// <param name="take">Número de registros a retornar.</param>
    /// <returns>Coleção de logs de auditoria.</returns>
    Task<IEnumerable<AuditLog>> GetByActionAsync(string action, int skip = 0, int take = 10);
}

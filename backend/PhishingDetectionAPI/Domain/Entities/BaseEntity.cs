namespace PhishingDetectionAPI.Domain.Entities;

/// <summary>
/// Classe base para todas as entidades do domínio.
/// Fornece propriedades comuns como ID e timestamps.
/// Implementa o Padrão de Entidade.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Identificador único da entidade (Primary Key).</summary>
    public int Id { get; set; }

    /// <summary>Data de criação da entidade (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Data da última atualização da entidade (UTC).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Verifica se a entidade é válida.
    /// Pode ser sobrescrita por entidades derivadas.
    /// </summary>
    /// <returns>True se a entidade é válida, False caso contrário.</returns>
    public virtual bool IsValid() => Id > 0;
}

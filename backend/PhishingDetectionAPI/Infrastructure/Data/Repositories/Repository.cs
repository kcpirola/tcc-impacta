namespace PhishingDetectionAPI.Infrastructure.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using PhishingDetectionAPI.Domain.Entities;
using PhishingDetectionAPI.Domain.Interfaces;

/// <summary>
/// Implementação genérica do padrão Repository.
/// Fornece operações CRUD básicas para todas as entidades.
/// Implementa o padrão Repository e o princípio de Responsabilidade Única (SRP) do SOLID.
/// </summary>
/// <typeparam name="TEntity">Tipo de entidade gerenciada.</typeparam>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly PhishingDetectionContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    /// <summary>Inicializa uma nova instância do repositório.</summary>
    /// <param name="context">Contexto do banco de dados.</param>
    public Repository(PhishingDetectionContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<TEntity>();
    }

    /// <summary>Obtém uma entidade pelo ID.</summary>
    public virtual async Task<TEntity?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>Obtém todas as entidades.</summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>Adiciona uma nova entidade.</summary>
    public virtual async Task AddAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>Atualiza uma entidade existente.</summary>
    public virtual async Task UpdateAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>Remove uma entidade.</summary>
    public virtual async Task RemoveAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>Remove uma entidade pelo ID.</summary>
    public virtual async Task RemoveAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            await RemoveAsync(entity);
        }
    }

    /// <summary>Verifica se uma entidade existe.</summary>
    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id);
    }
}

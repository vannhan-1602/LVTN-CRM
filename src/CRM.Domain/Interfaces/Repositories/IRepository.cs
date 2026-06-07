using System.Linq.Expressions;
using CRM.Domain.Common;

namespace CRM.Domain.Interfaces.Repositories;

public interface IRepository<TEntity, TKey>
    where TEntity : BaseEntity<TKey>
    where TKey : struct
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}

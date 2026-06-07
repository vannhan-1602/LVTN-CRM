using System.Linq.Expressions;
using CRM.Domain.Common;
using CRM.Domain.Interfaces.Repositories;
using CRM.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : BaseEntity<TKey>
    where TKey : struct
{
    protected readonly CrmDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(CrmDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default) =>
        await DbSet.FindAsync([id], cancellationToken);

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.ToListAsync(cancellationToken);

    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await DbSet.Where(predicate).ToListAsync(cancellationToken);

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        await DbSet.AddAsync(entity, cancellationToken);

    public virtual void Update(TEntity entity) => DbSet.Update(entity);

    public virtual void Remove(TEntity entity) => DbSet.Remove(entity);
}

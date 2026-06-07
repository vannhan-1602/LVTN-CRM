using CRM.Domain.Interfaces.Repositories;
using CRM.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CRM.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CrmDbContext _context;

    public UnitOfWork(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var efTx = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new EfTransaction(efTx);
    }
    public async Task<T> ExecuteInTransactionAsync<T>(
    Func<Task<T>> operation,
    CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await operation();
                await tx.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public void Dispose() => _context.Dispose();
}

internal sealed class EfTransaction : ITransaction
{
    private readonly IDbContextTransaction _tx;

    public EfTransaction(IDbContextTransaction tx) => _tx = tx;

    public Task CommitAsync(CancellationToken ct = default) => _tx.CommitAsync(ct);
    public Task RollbackAsync(CancellationToken ct = default) => _tx.RollbackAsync(ct);
    public ValueTask DisposeAsync() => _tx.DisposeAsync();
}
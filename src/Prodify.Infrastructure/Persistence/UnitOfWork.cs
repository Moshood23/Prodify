using Microsoft.EntityFrameworkCore.Storage;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ProdifyDbContext _dbContext;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(ProdifyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (_currentTransaction is not null)
                await _currentTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction is not null)
                await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }
}
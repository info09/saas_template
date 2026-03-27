using SaaS.Application.Interfaces;
using SaaS.Infrastructure.Repositories;

namespace SaaS.Infrastructure.Persistence;

public class TenantUnitOfWork : ITenantUnitOfWork
{
    private readonly TenantDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public TenantUnitOfWork(TenantDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var entityType = typeof(TEntity);
        if (_repositories.TryGetValue(entityType, out var repository))
        {
            return (IGenericRepository<TEntity>)repository;
        }

        var genericRepository = new GenericRepository<TEntity>(_context);
        _repositories[entityType] = genericRepository;
        return genericRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}

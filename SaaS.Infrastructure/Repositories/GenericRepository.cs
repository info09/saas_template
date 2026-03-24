using Microsoft.EntityFrameworkCore;
using SaaS.Application.Interfaces;
using SaaS.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace SaaS.Infrastructure.Repositories;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    private readonly TenantDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(TenantDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public IQueryable<TEntity> Query() => _dbSet.AsQueryable();

    public async Task<TEntity?> GetByIdAsync(object[] keyValues, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync(keyValues, cancellationToken);

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.Where(predicate).ToListAsync(cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        => await _dbSet.AddRangeAsync(entities, cancellationToken);

    public void Update(TEntity entity) => _dbSet.Update(entity);

    public void Remove(TEntity entity) => _dbSet.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities) => _dbSet.RemoveRange(entities);
}

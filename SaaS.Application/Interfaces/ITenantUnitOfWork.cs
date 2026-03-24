namespace SaaS.Application.Interfaces;

public interface ITenantUnitOfWork
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

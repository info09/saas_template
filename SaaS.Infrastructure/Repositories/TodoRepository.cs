using Microsoft.EntityFrameworkCore;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;
using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Repositories;

public class TodoRepository : ITodoRepository
{
    private readonly TenantDbContext _context;

    public TodoRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<TodoItem?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken)
    {
        return await _context.TodoItems
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
    }

    public async Task<(IReadOnlyCollection<TodoItem> Items, int TotalCount)> GetPaginatedListAsync(
        string userId, int pageNumber, int pageSize, bool? isCompleted, PriorityLevel? priority, CancellationToken cancellationToken)
    {
        var query = _context.TodoItems.Where(x => x.UserId == userId).AsQueryable();

        if (isCompleted.HasValue)
        {
            query = query.Where(x => x.IsCompleted == isCompleted.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(x => x.Priority == priority.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<TodoItem> AddAsync(TodoItem entity, CancellationToken cancellationToken)
    {
        _context.TodoItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(TodoItem entity, CancellationToken cancellationToken)
    {
        _context.TodoItems.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TodoItem entity, CancellationToken cancellationToken)
    {
        _context.TodoItems.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

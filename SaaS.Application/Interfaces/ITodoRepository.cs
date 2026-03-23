using SaaS.Domain.Entities;

namespace SaaS.Application.Interfaces;

public interface ITodoRepository
{
    Task<TodoItem?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken);

    Task<(IReadOnlyCollection<TodoItem> Items, int TotalCount)> GetPaginatedListAsync(
        string userId, int pageNumber, int pageSize, Domain.Enums.TodoStatus? status, Domain.Enums.PriorityLevel? priority, CancellationToken cancellationToken);

    Task<TodoItem> AddAsync(TodoItem entity, CancellationToken cancellationToken);
    Task UpdateAsync(TodoItem entity, CancellationToken cancellationToken);
    Task DeleteAsync(TodoItem entity, CancellationToken cancellationToken);
}

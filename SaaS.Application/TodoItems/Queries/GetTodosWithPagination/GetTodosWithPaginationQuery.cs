using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;
using SaaS.Domain.Enums;

namespace SaaS.Application.TodoItems.Queries.GetTodosWithPagination;

public record GetTodosWithPaginationQuery : IRequest<PaginatedList<TodoItemDto>>
{
    public string UserId { get; set; } = string.Empty; // Injected by Controller
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    
    public TodoStatus? Status { get; init; }
    public PriorityLevel? Priority { get; init; }
}

public class GetTodosWithPaginationQueryHandler : IRequestHandler<GetTodosWithPaginationQuery, PaginatedList<TodoItemDto>>
{
    private readonly ITodoRepository _repository;

    public GetTodosWithPaginationQueryHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedList<TodoItemDto>> Handle(GetTodosWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPaginatedListAsync(
            request.UserId,
            request.PageNumber,
            request.PageSize,
            request.Status,
            request.Priority,
            cancellationToken);

        var dtos = items.Select(x => new TodoItemDto
        {
            Id = x.Id,
            Title = x.Title,
            Note = x.Note,
            Priority = x.Priority,
            DueDate = x.DueDate,
            Status = x.Status,
            AssignedToUserId = x.AssignedToUserId,
            CreatedAt = x.CreatedAt
        }).ToList();

        return new PaginatedList<TodoItemDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}

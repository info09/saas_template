using MediatR;
using SaaS.Application.Interfaces;
using SaaS.Domain.Enums;

namespace SaaS.Application.TodoItems.Commands.UpdateTodoItemStatus;

public record UpdateTodoItemStatusCommand : IRequest
{
    public int Id { get; init; }
    public string UserId { get; set; } = string.Empty; // Injected by Controller
    public TodoStatus Status { get; init; }
}

public class UpdateTodoItemStatusCommandHandler : IRequestHandler<UpdateTodoItemStatusCommand>
{
    private readonly ITodoRepository _repository;

    public UpdateTodoItemStatusCommandHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(UpdateTodoItemStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, request.UserId, cancellationToken);

        if (entity == null)
            throw new Exception($"TodoItem {request.Id} not found.");

        entity.Status = request.Status;
        entity.LastModifiedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, cancellationToken);
    }
}

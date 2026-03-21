using MediatR;
using SaaS.Application.Interfaces;

namespace SaaS.Application.TodoItems.Commands.DeleteTodoItem;

public record DeleteTodoItemCommand(int Id, string UserId) : IRequest;

public class DeleteTodoItemCommandHandler : IRequestHandler<DeleteTodoItemCommand>
{
    private readonly ITodoRepository _repository;

    public DeleteTodoItemCommandHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, request.UserId, cancellationToken);
        if (entity == null)
        {
            throw new KeyNotFoundException($"TodoItem with ID {request.Id} not found.");
        }

        await _repository.DeleteAsync(entity, cancellationToken);
    }
}

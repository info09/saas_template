using MediatR;
using SaaS.Application.Interfaces;

namespace SaaS.Application.TodoItems.Commands.AssignTodoItem;

public record AssignTodoItemCommand : IRequest
{
    public int Id { get; init; }
    public string UserId { get; set; } = string.Empty; // Injected by Controller
    public string? AssignedToUserId { get; init; }
}

public class AssignTodoItemCommandHandler : IRequestHandler<AssignTodoItemCommand>
{
    private readonly ITodoRepository _repository;
    private readonly IIdentityService _identityService;

    public AssignTodoItemCommandHandler(ITodoRepository repository, IIdentityService identityService)
    {
        _repository = repository;
        _identityService = identityService;
    }

    public async Task Handle(AssignTodoItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, request.UserId, cancellationToken);

        if (entity == null)
            throw new Exception($"TodoItem {request.Id} not found.");

        // Optional: Verify if user exists in the tenant
        // Since we don't have a GetUserById in IIdentityService yet, 
        // we might want to skip or add it. But for now, we'll just set it.
        
        entity.AssignedToUserId = request.AssignedToUserId;
        entity.LastModifiedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, cancellationToken);
    }
}

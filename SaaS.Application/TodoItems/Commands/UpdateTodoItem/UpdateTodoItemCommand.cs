using FluentValidation;
using MediatR;
using SaaS.Application.Interfaces;
using SaaS.Domain.Enums;

namespace SaaS.Application.TodoItems.Commands.UpdateTodoItem;

public record UpdateTodoItemCommand : IRequest
{
    public int Id { get; init; }
    public string UserId { get; set; } = string.Empty; // Injected by Controller
    public string Title { get; init; } = string.Empty;
    public string? Note { get; init; }
    public PriorityLevel Priority { get; init; } = PriorityLevel.None;
    public DateTime? DueDate { get; init; }
    public bool IsCompleted { get; init; }
}

public class UpdateTodoItemCommandHandler : IRequestHandler<UpdateTodoItemCommand>
{
    private readonly ITodoRepository _repository;

    public UpdateTodoItemCommandHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(UpdateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, request.UserId, cancellationToken);
        if (entity == null)
        {
            throw new KeyNotFoundException($"TodoItem with ID {request.Id} not found.");
        }

        entity.Title = request.Title;
        entity.Note = request.Note;
        entity.Priority = request.Priority;
        entity.DueDate = request.DueDate;
        entity.IsCompleted = request.IsCompleted;
        entity.LastModifiedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, cancellationToken);
    }
}

public class UpdateTodoItemCommandValidator : AbstractValidator<UpdateTodoItemCommand>
{
    public UpdateTodoItemCommandValidator()
    {
        RuleFor(v => v.Title)
            .MaximumLength(200)
            .NotEmpty()
            .WithMessage("Title is required and must not exceed 200 characters.");
            
        RuleFor(v => v.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}

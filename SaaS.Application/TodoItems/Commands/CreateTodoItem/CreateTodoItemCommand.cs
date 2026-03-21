using FluentValidation;
using MediatR;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Application.TodoItems.Commands.CreateTodoItem;

public record CreateTodoItemCommand : IRequest<int>
{
    public string UserId { get; set; } = string.Empty; // Injected by Controller
    public string Title { get; init; } = string.Empty;
    public string? Note { get; init; }
    public PriorityLevel Priority { get; init; } = PriorityLevel.None;
    public DateTime? DueDate { get; init; }
}

public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, int>
{
    private readonly ITodoRepository _repository;

    public CreateTodoItemCommandHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var entity = new TodoItem
        {
            UserId = request.UserId,
            Title = request.Title,
            Note = request.Note,
            Priority = request.Priority,
            DueDate = request.DueDate,
            IsCompleted = false
        };

        await _repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}

public class CreateTodoItemCommandValidator : AbstractValidator<CreateTodoItemCommand>
{
    public CreateTodoItemCommandValidator()
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

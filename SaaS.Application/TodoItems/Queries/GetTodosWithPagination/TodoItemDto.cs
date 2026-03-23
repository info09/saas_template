using SaaS.Domain.Enums;

namespace SaaS.Application.TodoItems.Queries.GetTodosWithPagination;

public class TodoItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Note { get; set; }
    public PriorityLevel Priority { get; set; }
    public TodoStatus Status { get; set; }
    public string? AssignedToUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

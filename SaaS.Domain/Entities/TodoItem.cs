using SaaS.Domain.Enums;

namespace SaaS.Domain.Entities;

public class TodoItem
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    public string? Note { get; set; }
    
    public PriorityLevel Priority { get; set; } = PriorityLevel.None;
    
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }
}

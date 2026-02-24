namespace Backend.Domain.Common;

public abstract class BaseEntity
{
    // Identity
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
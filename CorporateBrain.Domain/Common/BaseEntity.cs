namespace CorporateBrain.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid(); // Unique ID
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Always use UTC for consistency
    public DateTime UpdatedAt { get; set; } // Nullable because it's not updated yet
}

namespace OneStep.Domain.Common;

/// <summary>
/// Base class for every entity that has its own Id column.
/// CreatedAt and UpdatedAt are filled in automatically when the entity is saved.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

using BuildingBlocks.Abstractions.Entities;

namespace BuildingBlocks.Abstractions;

public class Entity<T> : IEntity<T>
{
    public T Id { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; } = DateTime.UtcNow;
    public string? LastModifiedBy { get; set; }
}
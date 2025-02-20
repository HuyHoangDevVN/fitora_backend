using BuildingBlocks.Abstractions.Entities;

namespace BuildingBlocks.Abstractions;

public class EntityAuditBase<T> : EntityBase<T>, IEntityAuditBase<T>
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
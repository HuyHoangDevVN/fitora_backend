using BuildingBlocks.Abstractions;

namespace UserService.Domain.Models;

public class Group : Entity<Guid>
{
    public string GroupName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
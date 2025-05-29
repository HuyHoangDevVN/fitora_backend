using BuildingBlocks.Abstractions;

namespace UserService.Domain.Models;

public class GroupPost : Entity<Guid>
{
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public Guid PostId { get; set; }   
    public bool IsApproved { get; set; } = true;   
}
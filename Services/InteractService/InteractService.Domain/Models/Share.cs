using BuildingBlocks.Abstractions;
using InteractService.Domain.Enums;

namespace InteractService.Domain.Models;

public class Share : Entity<Guid>
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public Guid OriginalPostId { get; set; }
    public Post Post { get; set; } = null!;
    public Post OriginalPost { get; set; } = null!;
    public ShareTo ShareTo { get; set; } = ShareTo.Everyone;
}
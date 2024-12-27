using BuildingBlocks.Abstractions;
using InteractService.Domain.Enums;

namespace InteractService.Domain.Models;

public class Share : Entity<int>
{
    public int UserId { get; set; }
    public int PostId { get; set; }
    public int OriginalPostId { get; set; }
    public Post Post { get; set; } = null!;
    public Post OriginalPost { get; set; } = null!;
    public ShareTo ShareTo { get; set; } = ShareTo.Everyone;
}
using BuildingBlocks.Abstractions;
using BuildingBlocks.Abstractions.Entities;
using InteractService.Domain.Enums;

namespace InteractService.Domain.Models;

public class Post : Entity<Guid>, ISoftDelete
{
    public Guid UserId { get; set; }
    public Guid? GroupId { get; set; } = null;
    public string Content { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public PrivacyPost Privacy { get; set; } = PrivacyPost.Public;
    public bool IsDeleted { get; set; } = false;
}
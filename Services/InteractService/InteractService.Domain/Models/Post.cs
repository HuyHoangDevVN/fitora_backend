using BuildingBlocks.Abstractions;
using BuildingBlocks.Abstractions.Entities;
using InteractService.Domain.Enums;

namespace InteractService.Domain.Models;

public class Post : Entity<int>, ISoftDelete
{
    public int UserId { get; set; }
    public int? GroupId { get; set; } = null;
    public string Content { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public PrivacyPost Privacy { get; set; } = PrivacyPost.Public;
    public bool IsDeleted { get; set; } = false;
}
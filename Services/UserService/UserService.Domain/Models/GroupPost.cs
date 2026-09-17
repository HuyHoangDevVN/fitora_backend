using BuildingBlocks.Abstractions;
using UserService.Domain.Enums;

namespace UserService.Domain.Models;

public class GroupPost : Entity<Guid>
{
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public Guid PostId { get; set; }
    public bool IsApproved { get; set; } = true;

    // Moderation (26.3)
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Approved;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
}
namespace AuthService.Domain.Models;

using BuildingBlocks.Abstractions;

public enum VerificationStatus { Pending, Verified, Rejected }

public class IdentityVerification : Entity<Guid>
{
    public string UserId { get; set; } = default!;
    public string DocumentType { get; set; } = "CCCD";
    public string? DocumentNumber { get; set; }
    public string? FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? FrontImageUrl { get; set; }
    public string? BackImageUrl { get; set; }
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    public string? RejectionReason { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

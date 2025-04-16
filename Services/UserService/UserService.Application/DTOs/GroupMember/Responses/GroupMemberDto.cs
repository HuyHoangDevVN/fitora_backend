using UserService.Domain.Enums;

namespace UserService.Application.DTOs.GroupMember.Responses;

public class GroupMemberDto
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string? GroupName { get; set; } = string.Empty;
    public string? GroupDescription { get; set; } = string.Empty;
    public string? GroupPictureUrl { get; set; } = string.Empty;
    public string? GroupBackgroundPictureUrl { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string? UserName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? FirstName { get; set; } = String.Empty;
    public string? LastName { get; set; } = String.Empty;
    public DateTime? BirthDate { get; set; } = DateTime.Today;
    public Gender Gender { get; set; } = Gender.Unknown;
    public string? Address { get; set; } = String.Empty;
    public string? PhoneNumber { get; set; } = String.Empty;
    public string? ProfilePictureUrl { get; set; } = String.Empty;
    public string? ProfileBackgroundPictureUrl { get; set; } = String.Empty;
    public string? Bio { get; set; } = String.Empty;
    public GroupRole Role { get; set; } = GroupRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
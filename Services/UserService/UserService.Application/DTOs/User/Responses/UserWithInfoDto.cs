using UserService.Domain.Enums;

namespace UserService.Application.DTOs.User.Responses;

public class UserWithInfoDto
{
    public Guid Id { get; set; }
    public bool? IsFriend { get; set; }
    public bool? IsFollowing { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public Gender Gender { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
}
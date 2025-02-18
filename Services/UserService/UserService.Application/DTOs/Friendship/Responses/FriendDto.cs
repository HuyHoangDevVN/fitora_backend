using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Friendship.Responses;

public class FriendDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public Gender Gender { get; set; }
    public string? Address { get; set; } = String.Empty;
    public string? PhoneNumber { get; set; } = String.Empty;
    public string? ProfilePictureUrl { get; set; } = String.Empty;
    public string? Bio { get; set; } = String.Empty;
}
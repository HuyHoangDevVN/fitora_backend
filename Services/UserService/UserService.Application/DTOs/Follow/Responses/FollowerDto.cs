using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Follow.Responses;

public class FollowerDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? ProfilePictureUrl { get; set; } = String.Empty;
}
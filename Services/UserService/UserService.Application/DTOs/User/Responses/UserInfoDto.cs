using UserService.Domain.Enums;

namespace UserService.Application.DTOs.User.Responses;

public class UserInfoDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid UserId { get; set; } = Guid.Empty;
    public string FirstName { get; set; } = String.Empty;
    public string LastName { get; set; } = String.Empty;
    public DateTime BirthDate { get; set; } = DateTime.Today;
    public Gender Gender { get; set; } = Gender.Unknown;
    public string? Address { get; set; } = String.Empty;
    public string? PhoneNumber { get; set; } = String.Empty;
    public string? ProfilePictureUrl { get; set; } = String.Empty;
    public string? ProfileBackgroundPictureUrl { get; set; } = String.Empty;
    public string? Bio { get; set; } = String.Empty;
}
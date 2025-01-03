namespace UserService.Application.DTOs.User.Requests;

public class UserRegisteredMessageDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
}

namespace AuthService.Application.DTOs.Auth.Requests;

public class UserRegisteredMessageDto
{
    public string Email { get; set; }
    public string FullName { get; set; }
    public string UserId { get; set; }
}

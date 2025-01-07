namespace UserService.Application.DTOs.User.Responses;

public class UserDto
{
    public string Email { get; set; }
    public string UserName { get; set; }
    public UserInfo UserInfo { get; set; }
};
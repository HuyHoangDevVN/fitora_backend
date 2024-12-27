namespace UserService.Application.DTOs.User.Requests;

public record CreateUserRequest(Guid UserId, string Email, string Username);
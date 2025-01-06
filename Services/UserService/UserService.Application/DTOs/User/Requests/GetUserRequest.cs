namespace UserService.Application.DTOs.User.Requests;

public record GetUserRequest(Guid? Id, string? Email, string? UserName);
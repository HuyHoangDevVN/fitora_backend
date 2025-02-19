namespace UserService.Application.DTOs.User.Requests;

public record GetUserRequest(Guid Id, Guid? GetId, string? Email, string? Username);
namespace AuthService.Application.DTOs.Roles.Requests;

public record AssignRoleRequestDto(string[] RoleNames, string Email);

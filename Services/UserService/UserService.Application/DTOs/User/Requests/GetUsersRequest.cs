namespace UserService.Application.DTOs.User.Requests;

public record GetUsersRequest(string? UserName, string? Email) : PaginationRequest;
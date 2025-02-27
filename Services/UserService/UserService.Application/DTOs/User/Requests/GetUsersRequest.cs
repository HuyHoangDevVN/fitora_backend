namespace UserService.Application.DTOs.User.Requests;

public record GetUsersRequest(string? Username, string? Email) : PaginationRequest;
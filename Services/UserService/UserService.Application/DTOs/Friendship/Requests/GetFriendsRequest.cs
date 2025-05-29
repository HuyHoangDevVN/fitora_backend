namespace UserService.Application.DTOs.Friendship.Requests;

public record GetFriendsRequest(Guid Id, string? KeySearch, int PageIndex = 0, int PageSize = 10);

public record GetFriendsQuery(string? KeySearch, int PageIndex = 0, int PageSize = 10);
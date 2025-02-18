namespace UserService.Application.DTOs.Friendship.Requests;

public record GetFriendsRequest(Guid Id, int PageIndex = 0, int PageSize = 10);
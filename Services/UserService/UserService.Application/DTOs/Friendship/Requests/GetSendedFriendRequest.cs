namespace UserService.Application.DTOs.Friendship.Requests;

public record GetSentFriendRequest(Guid Id, int PageIndex = 0, int PageSize = 10);
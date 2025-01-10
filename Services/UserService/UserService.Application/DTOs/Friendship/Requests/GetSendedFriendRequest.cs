namespace UserService.Application.DTOs.Friendship.Requests;

public record GetSentFriendRequest(Guid Id) : PaginationRequest;
namespace UserService.Application.DTOs.Friendship.Requests;

public record GetReceivedFriendRequest(Guid Id) : PaginationRequest;
namespace UserService.Application.DTOs.Friendship.Requests;

public record GetFriendsRequest(Guid Id) : PaginationRequest;
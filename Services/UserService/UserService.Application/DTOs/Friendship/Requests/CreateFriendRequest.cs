namespace UserService.Application.DTOs.Friendship.Requests;

public record CreateFriendRequest(Guid senderId, Guid receiverId);
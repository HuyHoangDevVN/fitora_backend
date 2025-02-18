namespace UserService.Application.DTOs.Follow.Requests;

public record FollowRequest(Guid FollowerId, Guid FollowedId);
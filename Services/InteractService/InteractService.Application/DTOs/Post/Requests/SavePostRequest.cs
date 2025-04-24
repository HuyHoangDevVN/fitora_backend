namespace InteractService.Application.DTOs.Post.Requests;

public record SavePostRequest(Guid UserId, Guid PostId);
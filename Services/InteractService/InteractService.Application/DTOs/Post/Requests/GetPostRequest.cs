namespace InteractService.Application.DTOs.Post.Requests;

public record GetPostRequest(Guid? Id, string Content, string MediaUrl);
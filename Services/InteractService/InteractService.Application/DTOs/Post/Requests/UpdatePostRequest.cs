namespace InteractService.Application.DTOs.Post.Requests;

public record UpdatePostRequest(string? Content, string? MediaUrl, int? Privacy, bool? IsApproved);
namespace InteractService.Application.DTOs.Category.Requests;

public record FollowCategoryRequest(Guid UserId, Guid CategoryId);
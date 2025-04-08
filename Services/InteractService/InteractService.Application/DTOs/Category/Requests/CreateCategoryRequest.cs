namespace InteractService.Application.DTOs.Category.Requests;

public record CreateCategoryRequest(string Name, string? Slug, string? Description, Guid? ParentId, Guid? UserId);

public record CreateCategoryFromBody(string Name, string? Slug, string? Description, Guid? ParentId);
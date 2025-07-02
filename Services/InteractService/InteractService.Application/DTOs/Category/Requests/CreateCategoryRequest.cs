namespace InteractService.Application.DTOs.Category.Requests;

public record CreateCategoryRequest(
    string Name,
    string? Slug,
    string? Description,
    string? Color,
    Guid? ParentId,
    Guid? UserId);

public record CreateCategoryFromBody(
    string Name,
    string? Slug,
    string? Description,
    string? Color,
    Guid? ParentId);
namespace InteractService.Application.DTOs.Category.Requests;

public record UpdateCategoryRequest(Guid Id,
    string Name,
    string Slug,
    string Description,
    string? Color,
    Guid? ParentId);
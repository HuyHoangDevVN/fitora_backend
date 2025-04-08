using BuildingBlocks.Pagination.Base;

namespace InteractService.Application.DTOs.Category.Requests;

public record GetCategoriesRequest(string? KeySearch) : PaginationRequest;

public record GetCategoriesFollowedRequest(Guid UserId, string? KeySearch) : PaginationRequest;
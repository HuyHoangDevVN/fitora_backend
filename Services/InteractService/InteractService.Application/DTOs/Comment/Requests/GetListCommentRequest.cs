using BuildingBlocks.Pagination.Base;

namespace InteractService.Application.DTOs.Comment.Requests;

public record GetListCommentRequest(Guid? UserId, Guid? PostId, Guid? ParentCommentId, string? Keysearch) : PaginationRequest;
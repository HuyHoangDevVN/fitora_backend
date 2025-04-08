using BuildingBlocks.Pagination.Cursor;
using InteractService.Application.DTOs.Comment.Requests;
using InteractService.Application.DTOs.Comment.Responses;

namespace InteractService.Application.Usecases.Comments.Queries.GetCommentReplies;

public record GetCommentRepliesQuery(GetCommentRepliesRequest Request) : IQuery<PaginatedCursorResult<CommentResponseDto>>;
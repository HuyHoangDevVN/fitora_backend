using BuildingBlocks.Pagination.Cursor;
using InteractService.Application.DTOs.Comment.Requests;
using InteractService.Application.DTOs.Comment.Responses;

namespace InteractService.Application.Usecases.Comments.Queries.GetCommentsByPost;

public record GetCommentsByPostQuery(GetPostCommentsRequest Request) : IQuery<PaginatedCursorResult<CommentResponseDto>>;
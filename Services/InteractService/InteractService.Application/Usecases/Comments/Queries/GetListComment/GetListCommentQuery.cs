using BuildingBlocks.Pagination.Base;
using InteractService.Application.DTOs.Comment.Requests;
using InteractService.Application.DTOs.Comment.Responses;

namespace InteractService.Application.Usecases.Comments.Queries.GetListComment;

public record GetListCommentQuery(GetListCommentRequest Request) : IQuery<PaginatedResult<CommentResponseDto>>;
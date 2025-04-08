using BuildingBlocks.Pagination.Cursor;
using InteractService.Application.DTOs.Comment.Responses;

namespace InteractService.Application.Usecases.Comments.Queries.GetCommentReplies;

public class GetCommentRepliesHandler(ICommentRepository commentRepo, IMapper mapper) : IQueryHandler<GetCommentRepliesQuery, PaginatedCursorResult<CommentResponseDto>>
{
    public async Task<PaginatedCursorResult<CommentResponseDto>> Handle(GetCommentRepliesQuery query, CancellationToken cancellationToken)
    {
        var comments = await commentRepo.GetReplies(query.Request);
        return comments;
    }
}
using BuildingBlocks.Pagination.Cursor;
using InteractService.Application.DTOs.Comment.Responses;

namespace InteractService.Application.Usecases.Comments.Queries.GetCommentsByPost;

public class GetCommentsByPostHandler(ICommentRepository commentRepo, IMapper mapper)
    : IQueryHandler<GetCommentsByPostQuery, PaginatedCursorResult<CommentResponseDto>>
{
    public async Task<PaginatedCursorResult<CommentResponseDto>> Handle(GetCommentsByPostQuery query,
        CancellationToken cancellationToken)
    {
        var comments = await commentRepo.GetByPost(query.Request);
        return comments;
    }
}
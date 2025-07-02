using BuildingBlocks.Pagination.Base;
using InteractService.Application.DTOs.Comment.Responses;

namespace InteractService.Application.Usecases.Comments.Queries.GetListComment;

public class GetListCommentHandler(ICommentRepository commentRepo) : IQueryHandler<GetListCommentQuery, PaginatedResult<CommentResponseDto>>
{
    public async Task<PaginatedResult<CommentResponseDto>> Handle(GetListCommentQuery request, CancellationToken cancellationToken)
    {
        var comments = await commentRepo.GetListAsync(request.Request);
        return comments;
    }
}
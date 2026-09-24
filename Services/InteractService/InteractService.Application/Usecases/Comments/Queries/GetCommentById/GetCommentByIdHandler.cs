using BuildingBlocks.Exceptions;

namespace InteractService.Application.Usecases.Comments.Queries.GetCommentById;

public record CommentLocationDto(Guid Id, Guid PostId, Guid? ParentCommentId);

public class GetCommentByIdHandler(ICommentRepository commentRepo) : IQueryHandler<GetCommentByIdQuery, CommentLocationDto>
{
    public async Task<CommentLocationDto> Handle(GetCommentByIdQuery query, CancellationToken cancellationToken)
    {
        var comment = await commentRepo.GetByIdAsync(query.Id)
            ?? throw new NotFoundException("Bình luận không tồn tại hoặc đã bị xóa");
        return new CommentLocationDto(comment.Id, comment.PostId, comment.ParentCommentId);
    }
}

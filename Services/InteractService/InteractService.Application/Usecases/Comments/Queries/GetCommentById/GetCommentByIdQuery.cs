using BuildingBlocks.CQRS;

namespace InteractService.Application.Usecases.Comments.Queries.GetCommentById;

public record GetCommentByIdQuery(Guid Id) : IQuery<CommentLocationDto>;

namespace InteractService.Application.Usecases.Posts.Queries.GetByIdPost;

public record GetPostByIdQuery(Guid Id) : IQuery<PostResponseDto>;
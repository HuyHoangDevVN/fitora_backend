namespace InteractService.Application.Usecases.Posts.Queries.GetAllPost;

public record GetAllPostQuery() : IQuery<IEnumerable<PostResponseDto>>;
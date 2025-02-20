namespace InteractService.Application.Usecases.Posts.Queries.GetNewfeed;

public record GetNewfeedQuery(Guid Id) : IQuery<IEnumerable<PostResponseDto>>;
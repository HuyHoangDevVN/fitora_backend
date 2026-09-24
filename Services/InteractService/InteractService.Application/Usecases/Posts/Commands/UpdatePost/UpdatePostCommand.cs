namespace InteractService.Application.Usecases.Posts.Commands.UpdatePost;

public record UpdatePostCommand(Guid Id, UpdatePostRequest Request, Guid CurrentUserId) : ICommand<PostResponseDto>;

namespace InteractService.Application.Usecases.Posts.Commands.UpdatePost;

public record UpdatePostCommand(Guid Id, UpdatePostRequest Request) : ICommand<PostResponseDto>;
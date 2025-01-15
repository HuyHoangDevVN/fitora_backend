namespace InteractService.Application.Usecases.Posts.Commands.CreatePost;

public record CreatePostCommand(CreatePostRequest Request) : ICommand<PostResponseDto>;
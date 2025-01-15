namespace InteractService.Application.Usecases.Posts.Commands.DeletePost;

public record DeletePostCommand(Guid Id) : ICommand<bool>;
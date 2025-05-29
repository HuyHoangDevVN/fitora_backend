using BuildingBlocks.DTOs;

namespace InteractService.Application.Usecases.Posts.Commands.VotePost;

public record VotePostCommand(VotePostRequest Request) : ICommand<ResponseDto>;
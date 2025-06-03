using BuildingBlocks.DTOs;

namespace InteractService.Application.Usecases.Posts.Commands.SyncAllPosts;

public record SyncAllPostsCommand(int BatchSize = 1000) : ICommand<ResponseDto>;
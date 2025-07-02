using BuildingBlocks.DTOs;

namespace InteractService.Application.Usecases.Posts.Commands.SyncAllPosts;

public class SyncAllPostsHandler(IPostRepository postRepo) : ICommandHandler<SyncAllPostsCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(SyncAllPostsCommand command, CancellationToken cancellationToken)
    {
        var result = await postRepo.SyncAllPostsToElasticsearchAsync(command.BatchSize);
        return new ResponseDto
        {
            IsSuccess = result,
            Message = result ? "All posts synced successfully." : "Failed to sync posts."
        };
    }
}
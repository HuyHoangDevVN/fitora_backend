using BuildingBlocks.DTOs;

namespace InteractService.Application.Usecases.Posts.Commands.VotePost;

public class VotePostHandler(IPostRepository postRepo) : ICommandHandler<VotePostCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(VotePostCommand request, CancellationToken cancellationToken)
    {
        var result = await postRepo.VoteAsync(request.Request);
        return new ResponseDto(IsSuccess: result, Message: result ? "Vote thành công !" : "Vote thất bại", Data: null);
    }
}
using BuildingBlocks.DTOs;

namespace InteractService.Application.Usecases.Posts.Commands.UnsavePost;

public class UnSavePostHandler(IPostRepository postRepo) : ICommandHandler<UnSavePostCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(UnSavePostCommand command, CancellationToken cancellationToken)
    {
        var result = await postRepo.UnSavePostAsync(command.Request);
        return new ResponseDto(IsSuccess: result, Message: result ? "UnSave thành công !" : "UnSave thất bại !");
    }
}
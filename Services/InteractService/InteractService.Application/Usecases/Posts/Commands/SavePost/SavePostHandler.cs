using BuildingBlocks.DTOs;

namespace InteractService.Application.Usecases.Posts.Commands.SavePost;

public class SavePostHandler(IPostRepository postRepo, IMapper mapper) : ICommandHandler<SavePostCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(SavePostCommand command, CancellationToken cancellationToken)
    {
        var result = await postRepo.SavePostAsync(command.Request);
        return new ResponseDto(IsSuccess: result, Message: result ? "Đã lưu thành công !" : "Lưu thất bại !",
            Data: null);
    }
}
using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupPost.Commands.DeleteGroupPost;

public class DeleteGroupPostHandler(IGroupPostRepository groupPostRepo)
    : ICommandHandler<DeleteGroupPostCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(DeleteGroupPostCommand request, CancellationToken cancellationToken)
    {
        var result = await groupPostRepo.DeleteAsync(request.Id);
        return new ResponseDto(result);
    }
}
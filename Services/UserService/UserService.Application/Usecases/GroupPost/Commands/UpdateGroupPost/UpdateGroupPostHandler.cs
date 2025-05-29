using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupPost.Commands.UpdateGroupPost;

public class UpdateGroupPostHandler(IGroupPostRepository groupPostRepo, IMapper mapper) : ICommandHandler<UpdateGroupPostCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(UpdateGroupPostCommand request, CancellationToken cancellationToken)
    {
        var groupPost = mapper.Map<Domain.Models.GroupPost>(request.Request);
        var result = await groupPostRepo.UpdateAsync(groupPost);
        return new ResponseDto(result);
    }
}
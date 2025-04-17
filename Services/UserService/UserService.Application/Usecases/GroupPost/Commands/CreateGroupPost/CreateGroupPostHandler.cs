using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupPost.Commands.CreateGroupPost;

public class CreateGroupPostHandler (IGroupPostRepository groupPostRepo, IMapper mapper) : ICommandHandler<CreateGroupPostCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(CreateGroupPostCommand command, CancellationToken cancellationToken)
    {
        var groupPost = mapper.Map<Domain.Models.GroupPost>(command.Request);
        var result = await groupPostRepo.CreateAsync(groupPost);
        return new ResponseDto(result);
    }
}
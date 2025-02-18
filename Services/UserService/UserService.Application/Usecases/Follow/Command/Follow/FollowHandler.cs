using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Follow.Command.Follow;

public class FollowHandler(IFollowRepository followRepository, IMapper mapper)
    : ICommandHandler<FollowCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(FollowCommand request, CancellationToken cancellationToken)
    {
        var result = await followRepository.FollowAsync(request.Request);
        return result;
    }
}
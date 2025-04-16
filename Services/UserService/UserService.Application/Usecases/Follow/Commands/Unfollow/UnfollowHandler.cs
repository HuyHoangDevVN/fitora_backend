using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Follow.Commands.Unfollow;

public class UnfollowHandler(IFollowRepository followRepository, IMapper mapper)
    : ICommandHandler<UnfollowCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(UnfollowCommand request, CancellationToken cancellationToken)
    {
        var result = await followRepository.UnfollowAsync(request.Request);
        return new ResponseDto(IsSuccess: result,
            Message: result ? "Hủy theo dõi thành công!" : "Hủy theo dõi thất bại!");
    }
}
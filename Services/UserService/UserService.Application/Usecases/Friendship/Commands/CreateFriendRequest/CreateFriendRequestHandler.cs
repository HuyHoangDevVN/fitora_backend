using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Friendship.Commands.CreateFriendRequest;

public class CreateFriendRequestHandler(IFriendshipRepository friendshipRepo, IMapper mapper)
    : ICommandHandler<CreateFriendRequestCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(CreateFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var result = await friendshipRepo.CreateFriendRequestAsync(request.Request);
        return result;
    }
}
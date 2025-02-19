using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Friendship.Command.AccpectFriendRequest;

public class AcceptFriendRequestHandler (IFriendshipRepository friendshipRepo, IMapper mapper) : ICommandHandler<AcceptFriendRequestCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(AcceptFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var isSuccess = await friendshipRepo.AcceptFriendRequestAsync(request.request);
        return new ResponseDto(IsSuccess: isSuccess, Message: isSuccess ? "Accpect Success" : "Accpect Failed");
    }
}
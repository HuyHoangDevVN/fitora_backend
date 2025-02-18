using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Friendship.Queries.GetFriendRequests.Received;

public class GetReceivedFriendRequestHandler(IFriendshipRepository friendshipRepo, IMapper mapper)
    : IQueryHandler<GetReceivedFriendRequestQuerry, ResponseDto>
{
    public async Task<ResponseDto> Handle(GetReceivedFriendRequestQuerry request,
        CancellationToken cancellationToken)
    {
        var result = await friendshipRepo.GetReceivedFriendRequestAsync(request.Request);
        return new ResponseDto(result, IsSuccess: true, Message: "Get Received Friend Request Success");
    }
}
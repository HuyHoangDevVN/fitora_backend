using BuildingBlocks.DTOs;
using UserService.Application.DTOs.Friendship.Responses;

namespace UserService.Application.Usecases.Friendship.Queries.GetFriendRequests.Sended;

public class GetSentFriendRequestHandler (IFriendshipRepository friendshipRepo, IMapper mapper) : IQueryHandler<GetSentFriendRequestQuerry, ResponseDto>
{
    public async Task<ResponseDto> Handle(GetSentFriendRequestQuerry request, CancellationToken cancellationToken)
    {
        var result = await friendshipRepo.GetSentFriendRequestAsync(request.Request);
        return new ResponseDto(result, IsSuccess: true, Message: "Get Sent Friend Request Success");
    }
}
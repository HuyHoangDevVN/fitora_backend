using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Friendship.Queries.GetFriends;

public class GetFriendsHandler (IFriendshipRepository friendshipRepo, IMapper mapper) : IQueryHandler<GetFriendsQuerry, ResponseDto>
{
    public async Task<ResponseDto> Handle(GetFriendsQuerry request, CancellationToken cancellationToken)
    {
        var result = await friendshipRepo.GetFriends(request.Request);
        return new ResponseDto(result);
    }
}
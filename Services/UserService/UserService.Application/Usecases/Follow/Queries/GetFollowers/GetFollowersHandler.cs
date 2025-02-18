using BuildingBlocks.DTOs;
using UserService.Application.Usecases.Friendship.Queries.GetFriends;

namespace UserService.Application.Usecases.Follow.Queries.GetFollowers;

public class GetFollowersHandler (IFollowRepository followRepo, IMapper mapper) : IQueryHandler<GetFollowersQuery, ResponseDto>
{
    public async Task<ResponseDto> Handle(GetFollowersQuery request, CancellationToken cancellationToken)
    {
        var result = await followRepo.GetFollowersAsync(request.Request, false);
        return new ResponseDto(result);
    }
}
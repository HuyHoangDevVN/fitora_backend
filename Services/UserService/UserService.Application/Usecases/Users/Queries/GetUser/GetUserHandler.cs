using UserService.Application.DTOs.Friendship.Requests;

namespace UserService.Application.Usecases.Users.Queries.GetUser;

public class GetUserHandler(IUserRepository userRepo, IFollowRepository followRepo, IMapper mapper)
    : IQueryHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var userResult = await userRepo.GetUser(request.Request);
        var followNumber = await followRepo.GetNumberFollower(userResult.Id);
        var realationShip =
            await userRepo.GetRelationshipAsync(new CreateFriendRequest(request.Request.Id, request.Request.GetId));
        var result = mapper.Map<UserDto>(userResult);
        result.FollowerCount = followNumber.NumberOfFollowers;
        result.FollowingCount = followNumber.NumberOfFollowed;
        result.Relationship = realationShip;
        return result;
    }
}
using UserService.Application.DTOs.Friendship.Requests;
using BuildingBlocks.Exceptions;

namespace UserService.Application.Usecases.Users.Queries.GetUser;

public class GetUserHandler(IUserRepository userRepo, IFollowRepository followRepo, IMapper mapper)
    : IQueryHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var userResult = await userRepo.GetUser(request.Request);
        if (userResult is null)
        {
            throw new NotFoundException("User not found");
        }

        var followNumber = await followRepo.GetNumberFollower(userResult.Id);
        var targetUserId = request.Request.GetId ?? request.Request.Id;
        var realationShip = targetUserId == request.Request.Id
            ? null
            : await userRepo.GetRelationshipAsync(new CreateFriendRequest(request.Request.Id, targetUserId));
        var result = mapper.Map<UserDto>(userResult);
        result.FollowerCount = followNumber.NumberOfFollowers;
        result.FollowingCount = followNumber.NumberOfFollowed;
        result.Relationship = realationShip;
        return result;
    }
}

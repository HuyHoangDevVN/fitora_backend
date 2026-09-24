using BuildingBlocks.RepositoryBase.EntityFramework;
using UserService.Application.DTOs.Friendship.Requests;
using UserService.Application.DTOs.Friendship.Responses;

namespace UserService.Application.Usecases.Friendship.Queries.GetRelationship;

public class GetRelationshipHandler(IUserRepository userRepository)
    : IQueryHandler<GetRelationshipQuery, RelationshipDto>
{
    public async Task<RelationshipDto> Handle(GetRelationshipQuery request, CancellationToken cancellationToken)
    {
        return await userRepository.GetRelationshipAsync(
            new CreateFriendRequest(request.CurrentUserId, request.TargetUserId));
    }
}

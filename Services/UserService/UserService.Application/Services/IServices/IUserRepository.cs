using BuildingBlocks.Pagination.Base;
using UserService.Application.DTOs.Friendship.Requests;
using UserService.Application.DTOs.Friendship.Responses;

namespace UserService.Application.Services.IServices;

public interface IUserRepository
{
    Task<bool> CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(UserInfoDto request);
    Task<User?> GetUser(GetUserRequest request);
    Task<PaginatedResult<UserWithInfoDto>> GetUsers(GetUsersRequest request);
    Task<RelationshipDto> GetRelationshipAsync(CreateFriendRequest request);

    Task<List<UserWithInfoDto>> GetUsersByIdsAsync(Guid? id, List<Guid> userIds);
}
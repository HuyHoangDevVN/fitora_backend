using InteractService.Application.DTOs.CallAPI.Friend.Responses;

namespace InteractService.Application.Services.IServices;

public interface IUserApiService
{
    Task<bool> CreateGroupPost(Guid groupId, Guid postId, bool isApproved, CancellationToken cancellationToken);
    Task<FriendResponse> GetFriend(string keySearch, int pageIndex, int pageSize, CancellationToken cancellationToken);
}
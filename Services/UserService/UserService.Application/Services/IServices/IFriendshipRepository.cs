using System.Collections;
using BuildingBlocks.DTOs;
using UserService.Application.DTOs.Friendship.Requests;
using UserService.Application.DTOs.Friendship.Responses;

namespace UserService.Application.Services.IServices;

public interface IFriendshipRepository
{
    Task<ResponseDto> CreateFriendRequestAsync(CreateFriendRequest request);
    Task<PaginatedResult<FriendRequestDto>> GetSentFriendRequestAsync(GetSentFriendRequest request);
    Task<PaginatedResult<FriendRequestDto>> GetReceivedFriendRequestAsync(GetReceivedFriendRequest request);
    Task<PaginatedResult<FriendDto>> GetFriends(GetFriendsRequest request);
    Task<bool> AcceptFriendRequestAsync(Guid requestId);
    Task<bool> DeleteFriendRequestAsync(Guid requestId);
}
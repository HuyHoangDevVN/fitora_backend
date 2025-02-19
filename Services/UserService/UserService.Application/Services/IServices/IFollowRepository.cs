using BuildingBlocks.DTOs;
using UserService.Application.DTOs.Follow.Requests;
using UserService.Application.DTOs.Follow.Responses;
using UserService.Application.DTOs.Friendship.Requests;

namespace UserService.Application.Services.IServices;

public interface IFollowRepository
{
    Task<ResponseDto> FollowAsync(FollowRequest request);
    Task<PaginatedResult<FollowerDto>> GetFollowersAsync(GetFollowersRequest request, bool isFollowing);
    Task<bool> UnfollowAsync(FollowRequest request);
    Task<NumberOfFollowDto> GetNumberFollower(Guid Id);
}
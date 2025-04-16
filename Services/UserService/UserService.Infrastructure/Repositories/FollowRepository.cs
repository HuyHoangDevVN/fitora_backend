using System.Linq.Expressions;
using AutoMapper;
using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.RepositoryBase.EntityFramework;
using UserService.Application.DTOs.Follow.Requests;
using UserService.Application.DTOs.Follow.Responses;
using UserService.Application.Services.IServices;

namespace UserService.Infrastructure.Repositories;

public class FollowRepository : IFollowRepository
{
    private readonly IRepositoryBase<Follow> _followRepo;
    private readonly IMapper _mapper;

    public FollowRepository(IRepositoryBase<Follow> followRepo,
        IMapper mapper)
    {
        _followRepo = followRepo;
        _mapper = mapper;
    }


    private static FollowerDto MapToFollowDto(Follow fl, bool isFollowing)
    {
        var follower = isFollowing ? fl.Followed : fl.Follower;

        return new FollowerDto
        {
            Id = follower?.Id ?? Guid.Empty,
            Username = follower?.Username ?? "Unknown",
            Email = follower?.Email ?? "Unknown",
            FirstName = follower?.UserInfo?.FirstName ?? "Unknown",
            LastName = follower?.UserInfo?.LastName ?? "Unknown",
            ProfilePictureUrl = follower?.UserInfo?.ProfilePictureUrl ?? string.Empty,
        };
    }

    public async Task<ResponseDto> FollowAsync(FollowRequest request)
    {
        ValidateFollowRequest(request);

        if (await IsAlreadyFollowingAsync(request.FollowedId, request.FollowerId))
        {
            throw new InvalidOperationException("Bạn đã theo dõi người này rồi!");
        }

        var follow = _mapper.Map<Follow>(request);
        await _followRepo.AddAsync(follow);

        var isSuccess = await _followRepo.SaveChangesAsync() > 0;
        return new ResponseDto(
            Data: null,
            IsSuccess: isSuccess,
            Message: isSuccess ? "Theo dõi thành công!" : "Theo dõi thất bại."
        );
    }

    private void ValidateFollowRequest(FollowRequest request)
    {
        if (request.FollowedId == Guid.Empty)
        {
            throw new ArgumentException("FollowedId cannot be empty", nameof(request.FollowedId));
        }

        if (request.FollowerId == Guid.Empty)
        {
            throw new ArgumentException("FollowerId cannot be empty", nameof(request.FollowerId));
        }
    }

    private async Task<bool> IsAlreadyFollowingAsync(Guid followedId, Guid followerId)
    {
        return await _followRepo.GetAsync(f => f.FollowedId == followedId && f.FollowerId == followerId) != null;
    }


    public async Task<PaginatedResult<FollowerDto>> GetFollowersAsync(GetFollowersRequest request, bool isFollowing)
    {
        var includes = new List<Expression<Func<Follow, object>>>
        {
            fs => fs.Follower,
            fs => fs.Follower.UserInfo,
            fs => fs.Followed,
            fs => fs.Followed.UserInfo
        };

        return await _followRepo.GetPageWithIncludesAsync(
            paginationRequest: new PaginationRequest(request.PageIndex, request.PageSize),
            selector: f => MapToFollowDto(f, isFollowing),
            conditions: f => isFollowing ? f.FollowerId == request.Id : f.FollowedId == request.Id,
            includes: includes,
            cancellationToken: CancellationToken.None
        );
    }

    public async Task<bool> UnfollowAsync(FollowRequest request)
    {
        ValidateFollowRequest(request);

        if (!await IsAlreadyFollowingAsync(request.FollowedId, request.FollowerId))
        {
            return false;
        }

        await _followRepo.DeleteAsync(f => f.FollowedId == request.FollowedId && f.FollowerId == request.FollowerId);
        var isSuccess = await _followRepo.SaveChangesAsync() > 0;
        return isSuccess;
    }

    public async Task<NumberOfFollowDto> GetNumberFollower(Guid Id)
    {
        var numberOfFollowers = await _followRepo.CountAsync(f => f.FollowedId == Id);
        var numberOfFollowed = await _followRepo.CountAsync(f => f.FollowerId == Id);

        return new NumberOfFollowDto
        {
            NumberOfFollowers = numberOfFollowers,
            NumberOfFollowed = numberOfFollowed
        };
    }
}
using System.Linq.Expressions;
using AutoMapper;
using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.RepositoryBase.EntityFramework;
using UserService.Application.DTOs.GroupInvite.Requests;
using UserService.Application.DTOs.GroupInvite.Responses;
using UserService.Application.Services.IServices;
using UserService.Domain.Enums;

namespace UserService.Infrastructure.Repositories;

public class GroupInviteRepository : IGroupInviteRepository
{
    private readonly IRepositoryBase<GroupInvite> _groupInviteRepo;
    private readonly IGroupMemberRepository _groupMemberRepo;
    private readonly IMapper _mapper;

    public GroupInviteRepository(IRepositoryBase<GroupInvite> groupInviteRepo, IGroupMemberRepository groupMemberRepo,
        IMapper mapper)
    {
        _groupInviteRepo = groupInviteRepo;
        _groupMemberRepo = groupMemberRepo;
        _mapper = mapper;
    }

    public async Task<ResponseDto> CreateAsync(CreateGroupInviteRequest request)
    {
        var groupInvite = _mapper.Map<GroupInvite>(request);
        await _groupInviteRepo.AddAsync(groupInvite);
        var result = await _groupInviteRepo.SaveChangesAsync() > 0;
        return new ResponseDto { IsSuccess = result, Data = groupInvite.Id };
    }

    public async Task<ResponseDto> CreateRangeAsync(CreateGroupInvitesRequest request)
    {
        var groupInvites = request.ReceiverUserIds.Select(receiverId => new GroupInvite
        {
            GroupId = request.GroupId,
            SenderUserId = request.SenderUserId,
            ReceiverUserId = receiverId
        }).ToList();

        await _groupInviteRepo.AddRangeAsync(groupInvites);
        var result = await _groupInviteRepo.SaveChangesAsync() > 0;
        return new ResponseDto { IsSuccess = result, Data = groupInvites.Select(gi => gi.Id) };
    }

    public async Task<bool> AcceptAsync(Guid id)
    {
        var groupInvite = await _groupInviteRepo.GetAsync(gi => gi.Id == id);
        if (groupInvite == null) return false;

        groupInvite.Status = StatusGroupInvite.Accepted;

        var newGroupMember = CreateGroupMember(groupInvite);
        await _groupMemberRepo.CreateAsync(newGroupMember);

        await _groupInviteRepo.UpdateAsync(gi => gi.Id == groupInvite.Id, groupInvite);
        return await _groupInviteRepo.SaveChangesAsync() > 0;
    }

    private static GroupMember CreateGroupMember(GroupInvite groupInvite)
    {
        return new GroupMember
        {
            UserId = groupInvite.ReceiverUserId,
            GroupId = groupInvite.GroupId,
            Role = GroupRole.Member,
            JoinedAt = DateTime.UtcNow
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await _groupInviteRepo.DeleteAsync(gi => gi.Id == id);
        return await _groupInviteRepo.SaveChangesAsync() > 0;
    }

    public async Task<ResponseDto> GetByIdAsync(Guid id)
    {
        var groupInvite = await _groupInviteRepo.GetWithIncludesAsync(
            gi => gi.Id == id,
            new List<Expression<Func<GroupInvite, object>>>
            {
                gi => gi.Group,
                gi => gi.SenderUser,
                gi => gi.ReceiverUser
            }
        );

        return new ResponseDto { IsSuccess = true, Data = _mapper.Map<GroupInviteDto>(groupInvite) };
    }

    public async Task<ResponseDto> GetSentListAsync(GetSentGroupInviteRequest request) =>
        await GetGroupInvitesAsync(
            request.PageIndex,
            request.PageSize,
            gi => gi.SenderUserId == request.Id
        );

    public async Task<ResponseDto> GetReceivedListAsync(GetReceivedGroupInviteRequest request) =>
        await GetGroupInvitesAsync(
            request.PageIndex,
            request.PageSize,
            gi => gi.ReceiverUserId == request.Id
        );

    public async Task<ResponseDto> GetsByGroupIdAsync(GetGroupInvitesRequest request) =>
        await GetGroupInvitesAsync(
            request.PageIndex,
            request.PageSize,
            gi => gi.GroupId == request.GroupId
        );

    private static GroupInviteDto MapToGroupInviteDto(GroupInvite gi) => new()
    {
        Id = gi.Id,
        GroupId = gi.GroupId,
        GroupName = gi.Group?.Name ?? "Unknown Group",
        GroupImageUrl = gi.Group?.AvatarUrl,
        SenderUserId = gi.SenderUserId,
        SenderName = gi.SenderUser?.Username ?? "Unknown Sender",
        SenderImageUrl = gi.SenderUser?.UserInfo?.ProfilePictureUrl ?? string.Empty,
        ReceiverUserId = gi.ReceiverUserId,
        ReceiverName = gi.ReceiverUser?.Username ?? "Unknown Receiver",
        ReceiverImageUrl = gi.ReceiverUser?.UserInfo?.ProfilePictureUrl ?? string.Empty,
        Status = gi.Status
    };

    private async Task<ResponseDto> GetGroupInvitesAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<GroupInvite, bool>> predicate)
    {
        var groupInvites = await _groupInviteRepo.GetPageWithIncludesAsync(
            new PaginationRequest(pageIndex, pageSize),
            gi => MapToGroupInviteDto(gi),
            predicate,
            includes: new List<Expression<Func<GroupInvite, object>>>
            {
                gi => gi.Group,
                gi => gi.SenderUser,
                gi => gi.ReceiverUser
            },
            cancellationToken: CancellationToken.None
        );

        return new ResponseDto { IsSuccess = true, Data = groupInvites };
    }
}
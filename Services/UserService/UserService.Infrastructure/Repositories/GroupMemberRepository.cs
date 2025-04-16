using System.Linq.Expressions;
using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.RepositoryBase.EntityFramework;
using Microsoft.EntityFrameworkCore;
using UserService.Application.DTOs.GroupMember.Requests;
using UserService.Application.DTOs.GroupMember.Responses;
using UserService.Application.Services.IServices;
using UserService.Domain.Enums;

namespace UserService.Infrastructure.Repositories;

public class GroupMemberRepository : IGroupMemberRepository
{
    private readonly IRepositoryBase<GroupMember> _groupMemberRepo;
    private readonly IRepositoryBase<Group> _groupRepo;

    public GroupMemberRepository(IRepositoryBase<GroupMember> groupMemberRepo,
        IRepositoryBase<Group> groupRepo)
    {
        _groupRepo = groupRepo;
        _groupMemberRepo = groupMemberRepo;
    }

    public async Task<GroupMemberDto> CreateAsync(GroupMember groupMember)
    {
        if (!await GroupExistsAsync(groupMember.GroupId))
            throw new Exception("Nhóm không tồn tại");

        await _groupMemberRepo.AddAsync(groupMember);
        if (!await SaveChangesAsync())
            throw new Exception("Tạo thành viên nhóm thất bại");

        
         var result = await _groupMemberRepo.GetWithIncludesAsync(
            gm => gm.Id == groupMember.Id,
            new List<Expression<Func<GroupMember, object>>>
            {
                gm => gm.Group,
                gm => gm.User,
                gm => gm.User.UserInfo
            }
        );
        return MapToGroupMemberDto(result);
    }

    public async Task<bool> CreateRangeAsync(List<GroupMember> groupMembers)
    {
        if (!groupMembers.Any())
            return false;

        await _groupMemberRepo.AddRangeAsync(groupMembers);
        return await SaveChangesAsync();
    }

    public async Task<ResponseDto> AssignRoleAsync(AssignRoleGroupMemberRequest request)
    {
        var assigner = await _groupMemberRepo.GetAsync(gm => gm.UserId == request.AssignedBy);
        if (assigner?.Role != GroupRole.Admin)
            return new ResponseDto(null, false, "Người gán quyền không hợp lệ");

        var groupMember = await _groupMemberRepo.GetAsync(gm => gm.Id == request.MemberId);
        if (groupMember == null || groupMember.GroupId != request.GroupId || groupMember.UserId == request.AssignedBy)
            return new ResponseDto(null, false, "Thành viên nhóm không hợp lệ");

        groupMember.Role = request.Role;
        await _groupMemberRepo.UpdateAsync(gm => gm.UserId == groupMember.UserId, groupMember);

        var success = await SaveChangesAsync();
        return new ResponseDto(null, success, success ? "Gán quyền thành công" : "Gán quyền thất bại");
    }

    public async Task<bool> DeleteAsync(Guid memberId)
    {
        await _groupMemberRepo.DeleteAsync(gm => gm.UserId == memberId);
        return await SaveChangesAsync();
    }

    public async Task<bool> DeleteRangeAsync(List<Guid> memberIds)
    {
        await _groupMemberRepo.DeleteRangeAsync(gm => memberIds.Contains(gm.UserId));
        return await SaveChangesAsync();
    }

    public async Task<PaginatedResult<GroupMemberDto>> GetByGroupIdAsync(GetByGroupId request)
    {
        var groupMembers = await _groupMemberRepo.GetPageWithIncludesAsync<GroupMemberDto>(
            new PaginationRequest(request.pageIndex, request.pageSize),
            gm => MapToGroupMemberDto(gm),
            gm => gm.GroupId == request.groupId,
            includes: new List<Expression<Func<GroupMember, object>>>
            {
                gm => gm.Group,
                gm => gm.User,
                gm => gm.User.UserInfo
            },
            cancellationToken: CancellationToken.None
        );

        return new PaginatedResult<GroupMemberDto>(
            request.pageIndex,
            request.pageSize,
            groupMembers.Count,
            groupMembers.Data
        );
    }

    public async Task<bool> IsMemberAsync(Guid groupId, Guid userId)
    {
        return await _groupMemberRepo.Query()
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
    }

    private static GroupMemberDto MapToGroupMemberDto(GroupMember gm) => new()
    {
        Id = gm.Id,
        GroupId = gm.GroupId,
        GroupName = gm.Group?.Name ?? "Unknown Group",
        GroupDescription = gm.Group?.Description ?? string.Empty,
        GroupPictureUrl = gm.Group?.AvatarUrl,
        GroupBackgroundPictureUrl = gm.Group?.CoverImageUrl,
        UserId = gm.UserId,
        UserName = gm.User?.Username ?? "Unknown User",
        Email = gm.User?.Email ?? string.Empty,
        FirstName = gm.User?.UserInfo?.FirstName ?? string.Empty,
        LastName = gm.User?.UserInfo?.LastName ?? string.Empty,
        BirthDate = gm.User?.UserInfo?.BirthDate,
        Gender = (Gender)gm.User?.UserInfo?.Gender!,
        Address = gm.User?.UserInfo?.Address ?? string.Empty,
        PhoneNumber = gm.User?.UserInfo?.PhoneNumber ?? string.Empty,
        ProfilePictureUrl = gm.User?.UserInfo?.ProfilePictureUrl ?? string.Empty,
        ProfileBackgroundPictureUrl = gm.User?.UserInfo?.ProfileBackgroundPictureUrl ?? string.Empty,
        Bio = gm.User?.UserInfo?.Bio ?? string.Empty,
        Role = gm.Role,
        JoinedAt = gm.JoinedAt
    };

    private async Task<bool> GroupExistsAsync(Guid groupId)
    {
        return await _groupRepo.GetAsync(g => g.Id == groupId) != null;
    }

    private async Task<bool> SaveChangesAsync()
    {
        return await _groupMemberRepo.SaveChangesAsync() > 0;
    }
}
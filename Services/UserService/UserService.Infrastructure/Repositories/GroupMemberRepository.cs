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
        // Self-assignment is never allowed
        if (request.MemberId == request.AssignedBy)
            return new ResponseDto(null, false, "Không thể tự gán quyền cho chính mình");

        var groupMember = await _groupMemberRepo.GetAsync(gm => gm.Id == request.MemberId);
        if (groupMember == null || groupMember.GroupId != request.GroupId)
            return new ResponseDto(null, false, "Thành viên nhóm không hợp lệ");
        if (groupMember.UserId == request.AssignedBy)
            return new ResponseDto(null, false, "Không thể tự gán quyền cho chính mình");

        // Assigner must be a member of the same group
        var assigner = await _groupMemberRepo.GetAsync(gm => gm.GroupId == request.GroupId && gm.UserId == request.AssignedBy);
        if (assigner == null)
            return new ResponseDto(null, false, "Người gán quyền không phải thành viên nhóm");

        // Only Owner or Admin can assign roles
        if (assigner.Role != GroupRole.Owner && assigner.Role != GroupRole.Admin)
            return new ResponseDto(null, false, "Bạn không có quyền gán vai trò (chỉ Owner/Admin)");

        // Moderator cannot assign any role — already covered above, but keep explicit
        // Admin cannot assign Owner/Admin, only Moderator/Member
        if (assigner.Role == GroupRole.Admin)
        {
            if (request.Role == GroupRole.Owner || request.Role == GroupRole.Admin)
                return new ResponseDto(null, false, "Admin chỉ được gán vai trò Moderator hoặc Member");
        }

        // Only Owner can assign Owner/Admin — enforced by the Admin check above
        // Moderator path already rejected; Member path already rejected

        groupMember.Role = request.Role;
        await _groupMemberRepo.UpdateAsync(gm => gm.UserId == groupMember.UserId, groupMember);

        var success = await SaveChangesAsync();
        return new ResponseDto(null, success, success ? "Gán quyền thành công" : "Gán quyền thất bại");
    }


    public async Task<bool> DeleteAsync(Guid memberId, Guid requestedBy)
    {
        if (memberId == requestedBy)
            throw new Exception("Không thể tự xóa chính mình khỏi nhóm");

        var target = await _groupMemberRepo.GetAsync(gm => gm.UserId == memberId);
        if (target == null)
            throw new Exception("Thành viên không tồn tại");

        var requester = await _groupMemberRepo.GetAsync(gm => gm.GroupId == target.GroupId && gm.UserId == requestedBy);
        if (requester == null)
            throw new Exception("Người thực hiện không phải thành viên nhóm");
        if (requester.Role == GroupRole.Moderator || requester.Role == GroupRole.Member)
            throw new Exception("Bạn không có quyền xóa thành viên (chỉ Owner/Admin)");

        // Role hierarchy: Owner(1) < Admin(2) < Moderator(3) < Member(4) — lower value = higher privilege
        // Owner can delete anyone except self (already checked)
        if (requester.Role == GroupRole.Owner)
        {
            // Owner can delete any role in the same group except self
        }
        else if (requester.Role == GroupRole.Admin)
        {
            // Admin can only delete Member/Moderator, not Owner or another Admin
            if (target.Role == GroupRole.Owner || target.Role == GroupRole.Admin)
                throw new Exception("Admin chỉ được xóa thành viên thường hoặc Moderator, không được xóa Owner/Admin khác");
        }

        await _groupMemberRepo.DeleteAsync(gm => gm.GroupId == target.GroupId && gm.UserId == memberId);
        return await SaveChangesAsync();
    }

    public async Task<bool> DeleteRangeAsync(List<Guid> memberIds)
    {
        await _groupMemberRepo.DeleteRangeAsync(gm => memberIds.Contains(gm.UserId));
        return await SaveChangesAsync();
    }

    public async Task<PaginatedResult<GroupMemberDto>> GetByGroupIdAsync(GetByGroupIdRequest request)
    {
        var groupMembers = await _groupMemberRepo.GetPageWithIncludesAsync<GroupMemberDto>(
            new PaginationRequest(request.PageIndex, request.PageSize),
            gm => MapToGroupMemberDto(gm),
            gm => gm.GroupId == request.GroupId,
            includes: new List<Expression<Func<GroupMember, object>>>
            {
                gm => gm.Group,
                gm => gm.User,
                gm => gm.User.UserInfo
            },
            cancellationToken: CancellationToken.None
        );

        return new PaginatedResult<GroupMemberDto>(
            request.PageIndex,
            request.PageSize,
            groupMembers.Count,
            groupMembers.Data
        );
    }

    public async Task<GroupMemberDto?> GetByIdAsync(Guid id, Guid groupId)
    {
        var isMember = await IsMemberAsync(groupId, id);
        if (!isMember)
            return null;
        var groupMember = await _groupMemberRepo.GetWithIncludesAsync(
            gm => gm.UserId == id && gm.GroupId == groupId,
            includes: new List<Expression<Func<GroupMember, object>>>
            {
                gm => gm.Group,
                gm => gm.User,
                gm => gm.User.UserInfo
            },
            cancellationToken: CancellationToken.None
        );

        return MapToGroupMemberDto(groupMember);
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
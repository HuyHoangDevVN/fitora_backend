using AutoMapper;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.RepositoryBase.EntityFramework;
using Microsoft.EntityFrameworkCore;
using UserService.Application.DTOs.Group.Requests;
using UserService.Application.DTOs.Group.Responses;
using UserService.Application.Services.IServices;
using UserService.Domain.Enums;

namespace UserService.Infrastructure.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly IRepositoryBase<Group> _groupRepo;
    private readonly IRepositoryBase<GroupMember> _groupMemberRepo;
    private readonly DbSet<Group> _groupDbSet;
    private readonly DbSet<GroupMember> _groupMemberDbSet;
    private readonly IMapper _mapper;

    public GroupRepository(IRepositoryBase<Group> groupRepo, IRepositoryBase<GroupMember> groupMemberRepo,
        ApplicationDbContext dbContext,
        IMapper mapper)
    {
        _groupRepo = groupRepo;
        _groupMemberRepo = groupMemberRepo;
        _mapper = mapper;
    }

    public async Task<bool> CreateAsync(Group group)
    {
        await _groupRepo.AddAsync(group);
        return await _groupRepo.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(Group group)
    {
        await _groupRepo.UpdateAsync(g => g.Id == group.Id, group);
        return await _groupRepo.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await _groupRepo.DeleteAsync(g => g.Id == id);
        return await _groupRepo.SaveChangesAsync() > 0;
    }

    public async Task<PaginatedResult<Group>> GetGroupsAsync(GetGroupsRequest request)
    {
        var keySearch = request.KeySearch?.Trim().ToLower();
        return await _groupRepo.GetPageAsync(
                new PaginationRequest(request.PageIndex, request.PageSize),
                CancellationToken.None,
                g => (keySearch == null || g.Name.ToLower().Contains(keySearch) ||
                      g.Description.ToLower().Contains(keySearch)) &&
                     (request.Privacy == null || g.Privacy == request.Privacy) &&
                     (request.Status == null || g.Status == request.Status)
            )
            ;
    }

    public async Task<PaginatedResult<Group>> GetManagedGroupsAsync(GetManagedGroupsRequest request)
    {
        var groupMembers = await _groupMemberRepo.FindAsync(gm => gm.UserId == request.UserId &&
                                                                  (gm.Role == GroupRole.Admin ||
                                                                   gm.Role == GroupRole.Owner));
        var groupIds = groupMembers.Select(gm => gm.GroupId).ToList();

        return await _groupRepo.GetPageAsync(
            new PaginationRequest(request.PageIndex, request.PageSize),
            CancellationToken.None,
            g => groupIds.Contains(g.Id)
        );
    }

    public async Task<PaginatedResult<Group>> GetJoinedGroupsAsync(GetJoinedGroupsRequest request)
    {
        var groupMembers = await _groupMemberRepo.FindAsync(gm => gm.UserId == request.UserId &&
                                                                  (request.IsAll || gm.Role == GroupRole.Member ||
                                                                   gm.Role == GroupRole.Moderator));
        var groupIds = groupMembers.Select(gm => gm.GroupId).ToList();

        return await _groupRepo.GetPageAsync(
            new PaginationRequest(request.PageIndex, request.PageSize),
            CancellationToken.None,
            g => groupIds.Contains(g.Id)
        );
    }

    public async Task<PaginatedResult<Group>> SearchGroupsAsync(SearchGroupsRequest request, Guid currentUserId)
    {
        var q = request.Query?.Trim().ToLower();
        // Collect group ids the current user belongs to (for private/secret visibility)
        var memberships = await _groupMemberRepo.FindAsync(gm => gm.UserId == currentUserId);
        var memberGroupIds = memberships.Select(gm => gm.GroupId).ToList();

        return await _groupRepo.GetPageAsync(
            new PaginationRequest(request.PageIndex, request.PageSize),
            CancellationToken.None,
            g =>
                // keyword filter (Name or Description contains query, case-insensitive)
                (string.IsNullOrEmpty(q) ||
                 g.Name.ToLower().Contains(q) ||
                 g.Description.ToLower().Contains(q))
                // privacy filter: public visible to all; private/secret only if caller is member
                && (g.Privacy == GroupPrivacy.Public || memberGroupIds.Contains(g.Id))
        );
    }

    public async Task<GroupDto?> GetGroupByIdAsync(Guid id)
    {
        var group = await _groupRepo.GetAsync(g => g.Id == id);
        var memberCount = await _groupMemberRepo.CountAsync(gm => gm.GroupId == id);
        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            Privacy = group.Privacy,
            RequirePostApproval = group.RequirePostApproval,
            CoverImageUrl = group.CoverImageUrl,
            AvatarUrl = group.AvatarUrl,
            Status = group.Status,
            MemberCount = memberCount
        };
    }
}
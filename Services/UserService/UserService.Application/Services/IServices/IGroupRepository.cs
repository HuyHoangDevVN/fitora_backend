using BuildingBlocks.Pagination.Base;
using UserService.Application.DTOs.Group.Requests;
using UserService.Application.DTOs.Group.Responses;

namespace UserService.Application.Services.IServices;

public interface IGroupRepository
{
    Task<bool> CreateAsync(Group group);
    Task<bool> UpdateAsync(Group group);
    Task<bool> DeleteAsync(Guid id);
    Task<PaginatedResult<Group>> GetGroupsAsync(GetGroupsRequest request);
    
    Task<PaginatedResult<Group>> GetManagedGroupsAsync(GetManagedGroupsRequest request);
    Task<PaginatedResult<Group>> GetJoinedGroupsAsync(GetJoinedGroupsRequest request);
    Task<GroupDto?> GetGroupByIdAsync(Guid id);
}
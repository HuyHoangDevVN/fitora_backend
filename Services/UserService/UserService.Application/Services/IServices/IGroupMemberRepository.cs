using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Base;
using UserService.Application.DTOs.GroupMember.Requests;
using UserService.Application.DTOs.GroupMember.Responses;

namespace UserService.Application.Services.IServices;

public interface IGroupMemberRepository
{
    Task<GroupMemberDto> CreateAsync(GroupMember groupMember);
    
    Task<bool> CreateRangeAsync(List<GroupMember> groupMembers);
    
    Task<ResponseDto> AssignRoleAsync(AssignRoleGroupMemberRequest request); 
    
    Task<bool> DeleteAsync(Guid memberId, Guid requestedBy);
    
    Task<bool> DeleteRangeAsync(List<Guid> memberIds);

    Task<PaginatedResult<GroupMemberDto>> GetByGroupIdAsync(GetByGroupIdRequest request); 

    Task<bool> IsMemberAsync(Guid groupId, Guid userId);
    Task<GroupMemberDto?> GetByIdAsync(Guid id, Guid groupId);
}

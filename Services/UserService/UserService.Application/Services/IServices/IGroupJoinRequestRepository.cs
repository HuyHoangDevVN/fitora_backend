using BuildingBlocks.Pagination.Base;

namespace UserService.Application.Services.IServices;

public interface IGroupJoinRequestRepository
{
    Task<bool> CreateAsync(GroupJoinRequest groupJoinRequest);
    Task<bool> ApproveRequestAsync(Guid requestId, Guid reviewedBy, string? reviewComment = null);
    Task<bool> RejectRequestAsync(Guid requestId, Guid reviewedBy, string? reviewComment = null);
    Task<GroupJoinRequest?> GetRequestByIdAsync(Guid requestId);
    Task<PaginatedResult<GroupJoinRequest>> GetPendingRequestsByGroupIdAsync(Guid groupId);
    Task<PaginatedResult<GroupJoinRequest>> GetRequestsByUserIdAsync(Guid userId);
}
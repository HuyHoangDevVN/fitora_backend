using BuildingBlocks.DTOs;
using UserService.Application.DTOs.GroupInvite.Requests;

namespace UserService.Application.Services.IServices;

public interface IGroupInviteRepository
{
    Task<ResponseDto> CreateAsync(CreateGroupInviteRequest request);
    Task<ResponseDto> CreateRangeAsync(CreateGroupInvitesRequest request);
    Task<bool> AcceptAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
    Task<ResponseDto> GetByIdAsync(Guid id);
    Task<ResponseDto> GetSentListAsync(GetSentGroupInviteRequest request);
    Task<ResponseDto> GetReceivedListAsync(GetReceivedGroupInviteRequest request);
    Task<ResponseDto> GetsByGroupIdAsync(GetGroupInvitesRequest request);
}
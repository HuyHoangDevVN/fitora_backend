using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Base;

namespace UserService.Application.Services.IServices;

// Mục 27.3 / 26.8: Bảng Block đã có DB nhưng chưa có Application/API.
// Repository mới, đối xứng với IFollowRepository.
public interface IBlockRepository
{
    Task<ResponseDto> BlockUserAsync(Guid blockerId, Guid blockedUserId);
    Task<ResponseDto> UnblockUserAsync(Guid blockerId, Guid blockedUserId);
    Task<PaginatedResult<UserService.Domain.Models.Block>> GetBlockedUsersAsync(Guid blockerId, int pageIndex, int pageSize);
}

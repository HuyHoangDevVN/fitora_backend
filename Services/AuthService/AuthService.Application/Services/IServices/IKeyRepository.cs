

using AuthService.Application.DTOs.Key;
using AuthService.Application.DTOs.Key.Requests;
using AuthService.Application.DTOs.Key.Responses;
using BuildingBlocks.Pagination.Base;

namespace AuthService.Application.Services.IServices;

public interface IKeyRepository<T>
{
    Task<T> CreateKeyAsync(CreateKeyRequestDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<KeyDto>> GetKeysByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Trả refresh-token thật của key hợp lệ cuối cùng (chỉ dùng nội bộ luồng Login —
    /// KHÔNG expose qua controller; API cho FE dùng GetKeysAsync với token đã mask).
    /// </summary>
    Task<string?> GetLastValidRefreshTokenAsync(string userId, CancellationToken cancellationToken = default);

    Task<RefreshTokenByUserResponseDto> RefreshTokenByUser(RefreshTokenByUserRequestDto dto,
        CancellationToken cancellationToken = default);

    Task<PaginatedResult<KeyDto>> GetKeysAsync(string userId, PaginationRequest paginationRequest,
        CancellationToken cancellationToken = default!);
}


using AuthService.Application.DTOs.Key;
using AuthService.Application.DTOs.Key.Requests;
using AuthService.Application.DTOs.Key.Responses;

namespace AuthService.Application.Services.IServices;

public interface IKeyRepository<T>
{
    Task<T> CreateKeyAsync(CreateKeyRequestDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<KeyDto>> GetKeysByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<RefreshTokenByUserResponseDto> RefreshTokenByUser(RefreshTokenByUserRequestDto dto,
        CancellationToken cancellationToken = default);

    Task<PaginatedResult<KeyDto>> GetKeysAsync(PaginationRequest paginationRequest,
        CancellationToken cancellationToken = default!);
}
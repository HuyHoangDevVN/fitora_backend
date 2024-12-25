
using AuthService.Application.DTOs.Users.Requests;
using AuthService.Application.DTOs.Users.Responses;

namespace AuthService.Application.Services.IServices;

public interface IUserRepository
{
    Task<GetUserResponseDto> GetUserAsync(GetUserRequestDto dto, CancellationToken cancellationToken = default!);
    Task<GetUserResponseDto> EditInForUserAsync(EditInForUserRequestDto dto, CancellationToken cancellationToken = default!);
    Task<PaginatedResult<GetUsersResponseDto>> GetUsersAsync(PaginationRequest paginationRequest,CancellationToken cancellationToken = default!);

}

using AuthService.Application.DTOs.Key;
using BuildingBlocks.Pagination.Base;

namespace AuthService.Application.Auths.Queries.GetKeys;

// UserId do controller lấy từ claim đăng nhập — FE không được tự chọn user khác (26.7).
public record GetKeysQuery(string UserId, PaginationRequest PaginationRequest) : IQuery<GetKeysResult>;

public record GetKeysResult(PaginatedResult<KeyDto> PaginatedResult);


using AuthService.Application.DTOs.Key;

namespace AuthService.Application.Auths.Queries.GetKeys;

public record GetKeysQuery(PaginationRequest PaginationRequest) : IQuery<GetKeysResult>;

public record GetKeysResult(PaginatedResult<KeyDto> PaginatedResult);

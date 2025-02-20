
using AuthService.Application.DTOs.Key;
using BuildingBlocks.Pagination.Base;

namespace AuthService.Application.Auths.Queries.GetKeys;

public record GetKeysQuery(PaginationRequest PaginationRequest) : IQuery<GetKeysResult>;

public record GetKeysResult(PaginatedResult<KeyDto> PaginatedResult);

using AuthService.Application.DTOs.Users.Responses;
using BuildingBlocks.Pagination;

namespace AuthService.Application.Auths.Queries.GetUsers;

public record GetUsersQuery(PaginationRequest PaginationRequest) : IQuery<GetUsersResult>;
public record GetUsersResult(PaginatedResult<GetUsersResponseDto> PaginatedResult);
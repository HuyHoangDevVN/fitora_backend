using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Users.Queries.SearchUsers;

public record SearchUsersQuery(string Query, int PageIndex = 1, int PageSize = 10)
    : IQuery<ResponseDto>;

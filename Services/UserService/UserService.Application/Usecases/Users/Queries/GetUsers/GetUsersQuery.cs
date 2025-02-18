using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Users.Queries.GetUsers;

public record GetUsersQuery(GetUsersRequest Request) : IQuery<ResponseDto>;
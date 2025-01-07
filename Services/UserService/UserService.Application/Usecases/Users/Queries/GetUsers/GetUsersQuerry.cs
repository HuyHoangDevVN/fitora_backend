using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Users.Queries.GetUsers;

public record GetUsersQuerry(GetUsersRequest Request) : IQuery<ResponseDto>;
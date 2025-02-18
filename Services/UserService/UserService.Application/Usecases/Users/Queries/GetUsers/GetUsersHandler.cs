using BuildingBlocks.DTOs;
using UserService.Application.Services;

namespace UserService.Application.Usecases.Users.Queries.GetUsers;

public class GetUsersHandler(IUserRepository userRepos, IMapper mapper) : IQueryHandler<GetUsersQuery,ResponseDto>
{
    public async Task<ResponseDto> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await userRepos.GetUsers(request.Request);
        return new ResponseDto(users);
    }
}
namespace UserService.Application.Usecases.Users.Queries.GetUser;

public class GetUserHandler(IUserRepository userRepo, IMapper mapper) : IQueryHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var userResult = await userRepo.GetUser(request.Request);
        var result =  mapper.Map<UserDto>(userResult);
        return result;
    }
}
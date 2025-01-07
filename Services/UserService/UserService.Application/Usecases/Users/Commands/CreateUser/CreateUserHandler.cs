namespace UserService.Application.Usecases.Users.Commands.CreateUser;

public class CreateUserHandler(IUserRepository userRepo, IMapper mapper): ICommandHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = mapper.Map<User>(request.Request);
        var isSuccess = await userRepo.CreateUserAsync(user);
        return (isSuccess ? mapper.Map<UserDto>(user) : null) ?? throw new InvalidOperationException();
    }
}
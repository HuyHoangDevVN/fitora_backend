namespace UserService.Application.Usecases.Users.Commands.UpdateUser;

public class UpdateUserHandler(IUserRepository userRepo, IMapper mapper) : ICommandHandler<UpdateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var isSuccess = await userRepo.UpdateUserAsync(request.Request);
        return (isSuccess ? mapper.Map<UserDto>(request.Request) : null)!;
    }
}
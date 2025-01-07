namespace UserService.Application.Usecases.Users.Commands.UpdateUser;

public class UpdateUserHandler(IUserRepository userRepo, IMapper mapper) : ICommandHandler<UpdateUserCommand, UserInfoDto>
{
    public async Task<UserInfoDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var isSuccess = await userRepo.UpdateUserAsync(request.Request);
        return (isSuccess ? (request.Request) : null);
    }
}
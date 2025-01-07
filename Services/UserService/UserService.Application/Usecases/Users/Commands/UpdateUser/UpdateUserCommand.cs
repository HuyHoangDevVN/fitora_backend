namespace UserService.Application.Usecases.Users.Commands.UpdateUser;

public record UpdateUserCommand(UserInfoDto Request) : ICommand<UserInfoDto>;
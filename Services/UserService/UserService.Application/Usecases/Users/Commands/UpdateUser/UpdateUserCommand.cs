namespace UserService.Application.Usecases.Users.Commands.UpdateUser;

public record UpdateUserCommand(UpdateUserRequest Request) : ICommand<UserDto>;
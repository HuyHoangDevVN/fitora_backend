namespace UserService.Application.Usecases.Users.Commands.CreateUser;

public record CreateUserCommand(CreateUserRequest Request) : ICommand<UserDto>;
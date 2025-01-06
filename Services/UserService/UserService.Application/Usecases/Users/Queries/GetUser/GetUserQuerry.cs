namespace UserService.Application.Usecases.Users.Queries.GetUser;

public record GetUserQuerry(GetUserRequest Request) : IQuery<UserDto>;
namespace UserService.Application.Usecases.Users.Queries.GetUser;

public record GetUserQuery(GetUserRequest Request) : IQuery<UserDto>;
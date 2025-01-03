using UserService.Application.Messaging.MessageHandlers.IHandlers;

namespace UserService.Application.Messaging.MessageHandlers;

public class UserRegisteredMessageHandler : IMessageHandler<UserRegisteredMessageDto>
{
    private readonly IUserRepository _userService;

    public UserRegisteredMessageHandler(IUserRepository userService)
    {
        _userService = userService;
    }

    public async Task HandleAsync(UserRegisteredMessageDto message)
    {
        Console.WriteLine("UserRegisteredMessageHandler");
        await _userService.CreateUserAsync(new User
        {
            Id = message.UserId,
            Email = message.Email,
        });
    }
}

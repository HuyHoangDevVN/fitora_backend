namespace AuthService.Domain.Events;

public class UserRegisteredEvent
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime RegisteredAt { get; set; }

    public UserRegisteredEvent(Guid userId, string name, string email, DateTime registeredAt)
    {
        UserId = userId;
        Name = name;
        Email = email;
        RegisteredAt = registeredAt;
    }
}
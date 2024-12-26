using BuildingBlocks.Abstractions;

namespace UserService.Domain.Models;

public class User : Entity<int>
{
    public string Username { get; set; }
    public string Email { get; set; }
    
    public UserInfo? UserInfo { get; set; }
}
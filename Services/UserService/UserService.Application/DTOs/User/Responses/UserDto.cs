using UserService.Application.DTOs.Friendship.Responses;

namespace UserService.Application.DTOs.User.Responses;

public class UserDto
{
    public string Email { get; set; }
    public string UserName { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public RelationshipDto? Relationship { get; set; }
    public UserInfo UserInfo { get; set; }
};
namespace UserService.Application.DTOs.Friendship.Responses;

public class RelationshipDto
{
    public bool IsFriend { get; set; } = false;
    public bool IsFriendRequest { get; set; } = false;
    public bool IsFollowing { get; set; } = false;
}
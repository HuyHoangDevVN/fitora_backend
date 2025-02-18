using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Friendship.Responses;

public class FriendRequestDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string SenderName { get; set; }
    public string? SenderImageUrl { get; set; }
    public string ReceiverName { get; set; }
    public string? ReceiverImageUrl { get; set; }
    public StatusFriendRequest? Status { get; set; }
    public DateTime? CreateDate { get; set; }
}
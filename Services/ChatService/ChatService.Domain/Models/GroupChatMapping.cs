namespace ChatService.Domain.Models;

public class GroupChatMapping
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CommunityGroupId { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

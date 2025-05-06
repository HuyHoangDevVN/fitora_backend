namespace ChatService.Application.Services;

public interface IChatRepository
{
    Task<string> CreateConversationAsync(List<string> participantIds, bool isGroup, Group groupInfo = null);
    Task SendMessageAsync(string senderId, string conversationId, string content, string type);
    Task<List<Message>> GetChatHistoryAsync(string conversationId);
    Task DeleteMessageAsync(string messageId);
    Task RecallMessageAsync(string messageId);
    Task AddReactionAsync(string messageId, string userId, string emoji);
    Task MarkAsReadAsync(string messageId, bool isRead);
    Task UpdateGroupInfoAsync(string conversationId, Group groupInfo);
    Task AddGroupMemberAsync(string conversationId, string userId);
    Task RemoveGroupMemberAsync(string conversationId, string userId);
    Task AssignGroupAdminAsync(string conversationId, string userId);
}
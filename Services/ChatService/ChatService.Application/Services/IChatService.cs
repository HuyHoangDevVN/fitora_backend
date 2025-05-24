using ChatService.Application.Data.Message.Request;

namespace ChatService.Application.Services;

public interface IChatService
{
    Task<string> CreateConversationAsync(List<string> participantIds, bool isGroup, Group groupInfo = null!);
    Task<List<Conversation>> GetGroupConversationsByUserIdAsync(string userId);
    Task<Conversation> GetPrivateConversationAsync(string userId, string otherUserId);
    Task<Message> SendMessageAsync(string senderId, string conversationId, string content, string type);
    Task<List<Message>> GetChatHistoryAsync(GetHistoryChatRequest request);
    Task<bool> DeleteMessageAsync(string messageId);
    Task<bool> RecallMessageAsync(GetHistoryChatRequest request);
    Task<bool> AddReactionAsync(string messageId, string userId, string emoji);
    Task<bool> MarkAsReadAsync(string messageId, bool isRead);
    Task<bool> UpdateGroupInfoAsync(string conversationId, Group groupInfo);
    Task<bool> AddGroupMemberAsync(string conversationId, string userId);
    Task<bool> RemoveGroupMemberAsync(string conversationId, string userId);
    Task<bool> AssignGroupAdminAsync(string conversationId, string userId);
}
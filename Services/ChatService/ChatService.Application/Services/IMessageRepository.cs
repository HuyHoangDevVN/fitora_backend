using ChatService.Application.Data.Message.Request;

namespace ChatService.Application.Services;

public interface IMessageRepository
{
    Task AddAsync(Message message);
    Task<List<Message>> GetByConversationIdAsync(GetHistoryChatRequest request);
    Task UpdateAsync(Message message);
    Task DeleteAsync(string messageId);
    Task MarkAsReadAsync(string messageId, bool isRead);
    Task AddReactionAsync(string messageId, Reaction reaction);
}
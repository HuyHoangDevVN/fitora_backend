using ChatService.Application.Data.Message.Request;
using ChatService.Application.Services;
using MongoDB.Driver;

namespace ChatService.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly IMongoCollection<Message> _messages;

    public MessageRepository(IMongoDatabase database)
    {
        _messages = database.GetCollection<Message>("messages");
    }

    public async Task AddAsync(Message message)
    {
        await _messages.InsertOneAsync(message);
    }

    public async Task<List<Message>> GetByConversationIdAsync(GetHistoryChatRequest request)
    {
        int skip = request.PageIndex * request.PageSize;
        return await _messages
            .Find(m => m.GroupId == request.ConversationId || m.ReceiverId == request.ConversationId)
            .Skip(skip)
            .Limit(request.PageSize)
            .ToListAsync();
    }

    public async Task UpdateAsync(Message message)
    {
        await _messages.ReplaceOneAsync(m => m.Id == message.Id, message);
    }

    public async Task DeleteAsync(string messageId)
    {
        await _messages.UpdateOneAsync(m => m.Id == messageId, Builders<Message>.Update.Set(m => m.IsDeleted, true));
    }

    public async Task MarkAsReadAsync(string messageId, bool isRead)
    {
        await _messages.UpdateOneAsync(m => m.Id == messageId, Builders<Message>.Update.Set(m => m.IsRead, isRead));
    }

    public async Task AddReactionAsync(string messageId, Reaction reaction)
    {
        await _messages.UpdateOneAsync(
            m => m.Id == messageId,
            Builders<Message>.Update.Push(m => m.Reactions, reaction));
    }
}
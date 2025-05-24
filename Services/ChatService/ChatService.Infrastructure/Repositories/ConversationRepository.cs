using ChatService.Application.Services;
using MongoDB.Driver;

namespace ChatService.Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly IMongoCollection<Conversation> _conversations;

    public ConversationRepository(IMongoDatabase database)
    {
        _conversations = database.GetCollection<Conversation>("conversations");
    }

    public async Task AddAsync(Conversation conversation)
    {
        await _conversations.InsertOneAsync(conversation);
    }

    public async Task UpdateAsync(Conversation conversation)
    {
        await _conversations.ReplaceOneAsync(c => c.Id == conversation.Id, conversation);
    }

    public async Task<Conversation> GetByIdAsync(string conversationId)
    {
        return await _conversations.Find(c => c.Id == conversationId).FirstOrDefaultAsync();
    }
    
    public async Task<List<Conversation>> GetGroupConversationsByUserIdAsync(string userId)
    {
        var filter = Builders<Conversation>.Filter.And(
            Builders<Conversation>.Filter.Eq(c => c.IsGroup, true),
            Builders<Conversation>.Filter.AnyEq(c => c.ParticipantIds, userId)
        );

        return await _conversations.Find(filter).ToListAsync();
    }
    public async Task<Conversation> GetPrivateConversationAsync(string userId, string otherUserId)
    {
        var filter = Builders<Conversation>.Filter.And(
            Builders<Conversation>.Filter.Eq(c => c.IsGroup, false),
            Builders<Conversation>.Filter.All(c => c.ParticipantIds, new[] { userId, otherUserId })
        );

        return await _conversations.Find(filter).FirstOrDefaultAsync();
    }

    public async Task AddMemberAsync(string conversationId, string userId)
    {
        await _conversations.UpdateOneAsync(
            c => c.Id == conversationId,
            Builders<Conversation>.Update.Push(c => c.GroupInfo.MemberIds, userId));
    }

    public async Task RemoveMemberAsync(string conversationId, string userId)
    {
        await _conversations.UpdateOneAsync(
            c => c.Id == conversationId,
            Builders<Conversation>.Update.Pull(c => c.GroupInfo.MemberIds, userId));
    }

    public async Task AssignAdminAsync(string conversationId, string userId)
    {
        await _conversations.UpdateOneAsync(
            c => c.Id == conversationId,
            Builders<Conversation>.Update.Push(c => c.GroupInfo.AdminIds, userId));
    }
}
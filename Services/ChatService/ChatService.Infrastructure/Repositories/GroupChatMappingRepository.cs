using ChatService.Application.Services;
using MongoDB.Driver;

namespace ChatService.Infrastructure.Repositories;

public class GroupChatMappingRepository : IGroupChatMappingRepository
{
    private readonly IMongoCollection<GroupChatMapping> _col;

    public GroupChatMappingRepository(IMongoDatabase database)
    {
        _col = database.GetCollection<GroupChatMapping>("groupChatMappings");
    }

    public async Task<GroupChatMapping?> GetByGroupIdAsync(string groupId)
        => await _col.Find(x => x.CommunityGroupId == groupId).FirstOrDefaultAsync();

    public async Task<GroupChatMapping?> GetByConversationIdAsync(string conversationId)
        => await _col.Find(x => x.ConversationId == conversationId).FirstOrDefaultAsync();

    public async Task AddAsync(GroupChatMapping mapping)
        => await _col.InsertOneAsync(mapping);
}

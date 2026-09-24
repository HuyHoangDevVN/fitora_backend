namespace ChatService.Application.Services;

public interface IGroupChatMappingRepository
{
    Task<GroupChatMapping?> GetByGroupIdAsync(string groupId);
    Task<GroupChatMapping?> GetByConversationIdAsync(string conversationId);
    Task AddAsync(GroupChatMapping mapping);
}

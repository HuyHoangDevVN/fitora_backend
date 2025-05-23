namespace ChatService.Application.Services;

public interface IConversationRepository
{
    Task AddAsync(Conversation conversation);
    Task UpdateAsync(Conversation conversation);
    Task<Conversation> GetByIdAsync(string conversationId);
    Task<List<Conversation>> GetGroupConversationsByUserIdAsync(string userId);

    Task<Conversation> GetPrivateConversationAsync(string userId, string otherUserId);
    Task AddMemberAsync(string conversationId, string userId);
    Task RemoveMemberAsync(string conversationId, string userId);
    Task AssignAdminAsync(string conversationId, string userId);
}
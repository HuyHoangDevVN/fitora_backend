using ChatService.Application.Services;

namespace ChatService.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;

    public ChatRepository(IMessageRepository messageRepository, IConversationRepository conversationRepository)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
    }

    public async Task<string> CreateConversationAsync(List<string> participantIds, bool isGroup, Group groupInfo = null)
    {
        var conversation = new Conversation
        {
            Id = Guid.NewGuid().ToString(),
            ParticipantIds = participantIds,
            CreatedAt = DateTime.UtcNow,
            IsGroup = isGroup,
            GroupInfo = isGroup ? groupInfo : null
        };
        await _conversationRepository.AddAsync(conversation);
        return conversation.Id;
    }

    public async Task SendMessageAsync(string senderId, string conversationId, string content, string type)
    {
        var message = new Message
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = senderId,
            GroupId = conversationId,
            Content = content,
            Type = type,
            Timestamp = DateTime.UtcNow
        };
        await _messageRepository.AddAsync(message);
    }

    public async Task<List<Message>> GetChatHistoryAsync(string conversationId)
    {
        return await _messageRepository.GetByConversationIdAsync(conversationId);
    }

    public async Task DeleteMessageAsync(string messageId)
    {
        await _messageRepository.DeleteAsync(messageId);
    }

    public async Task RecallMessageAsync(string messageId)
    {
        var messages = await _messageRepository.GetByConversationIdAsync(messageId);
        var message = messages.FirstOrDefault();
        if (message != null && !message.IsRecalled)
        {
            message.IsRecalled = true;
            await _messageRepository.UpdateAsync(message);
        }
    }

    public async Task AddReactionAsync(string messageId, string userId, string emoji)
    {
        var reaction = new Reaction { UserId = userId, Emoji = emoji };
        await _messageRepository.AddReactionAsync(messageId, reaction);
    }

    public async Task MarkAsReadAsync(string messageId, bool isRead)
    {
        await _messageRepository.MarkAsReadAsync(messageId, isRead);
    }

    public async Task UpdateGroupInfoAsync(string conversationId, Group groupInfo)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation != null && conversation.IsGroup)
        {
            conversation.GroupInfo = groupInfo;
            await _conversationRepository.UpdateAsync(conversation);
        }
    }

    public async Task AddGroupMemberAsync(string conversationId, string userId)
    {
        await _conversationRepository.AddMemberAsync(conversationId, userId);
    }

    public async Task RemoveGroupMemberAsync(string conversationId, string userId)
    {
        await _conversationRepository.RemoveMemberAsync(conversationId, userId);
    }

    public async Task AssignGroupAdminAsync(string conversationId, string userId)
    {
        await _conversationRepository.AssignAdminAsync(conversationId, userId);
    }
}
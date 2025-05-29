using ChatService.Application.Data.Message.Request;
using ChatService.Application.Services;
using ChatService.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Infrastructure.Repositories;

public class ChatService : IChatService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatService(IMessageRepository messageRepository, IConversationRepository conversationRepository,
        IHubContext<ChatHub> hubContext)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _hubContext = hubContext;
    }

    public async Task<string> CreateConversationAsync(List<string> participantIds, bool isGroup, Group groupInfo = null)
    {
        var conversation = new Conversation
        {
            Id = Guid.NewGuid().ToString(),
            ParticipantIds = participantIds,
            CreatedAt = DateTime.UtcNow,
            IsGroup = isGroup,
            GroupInfo = (isGroup ? groupInfo : null)!
        };
        await _conversationRepository.AddAsync(conversation);
        return conversation.Id;
    }

    public async Task<List<Conversation>> GetGroupConversationsByUserIdAsync(string userId)
    {
        return await _conversationRepository.GetGroupConversationsByUserIdAsync(userId);
    }

    public async Task<Conversation> GetPrivateConversationAsync(string userId, string otherUserId)
    {
        return await _conversationRepository.GetPrivateConversationAsync(userId, otherUserId);
    }

    public async Task<Message> SendMessageAsync(string senderId, string conversationId, string content, string type)
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

        // Lưu tin nhắn vào MongoDB
        await _messageRepository.AddAsync(message);

        // Gửi tin nhắn qua SignalR đến tất cả client trong conversation
        await _hubContext.Clients.Group(conversationId)
            .SendAsync("ReceiveMessage", senderId, conversationId, content, type);
        return message;
    }

    public async Task<List<Message>> GetChatHistoryAsync(GetHistoryChatRequest request)
    {
        return await _messageRepository.GetByConversationIdAsync(request);
    }

    public async Task<bool> DeleteMessageAsync(string messageId)
    {
        try
        {
            await _messageRepository.DeleteAsync(messageId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RecallMessageAsync(GetHistoryChatRequest request)
    {
        try
        {
            var messages = await _messageRepository.GetByConversationIdAsync(request);
            var message = messages.FirstOrDefault();
            if (message != null && !message.IsRecalled)
            {
                message.IsRecalled = true;
                await _messageRepository.UpdateAsync(message);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AddReactionAsync(string messageId, string userId, string emoji)
    {
        try
        {
            var reaction = new Reaction { UserId = userId, Emoji = emoji };
            await _messageRepository.AddReactionAsync(messageId, reaction);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> MarkAsReadAsync(string messageId, bool isRead)
    {
        try
        {
            await _messageRepository.MarkAsReadAsync(messageId, isRead);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateGroupInfoAsync(string conversationId, Group groupInfo)
    {
        try
        {
            var conversation = await _conversationRepository.GetByIdAsync(conversationId);
            if (conversation.IsGroup)
            {
                conversation.GroupInfo = groupInfo;
                await _conversationRepository.UpdateAsync(conversation);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AddGroupMemberAsync(string conversationId, string userId)
    {
        try
        {
            await _conversationRepository.AddMemberAsync(conversationId, userId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveGroupMemberAsync(string conversationId, string userId)
    {
        try
        {
            await _conversationRepository.RemoveMemberAsync(conversationId, userId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AssignGroupAdminAsync(string conversationId, string userId)
    {
        try
        {
            await _conversationRepository.AssignAdminAsync(conversationId, userId);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

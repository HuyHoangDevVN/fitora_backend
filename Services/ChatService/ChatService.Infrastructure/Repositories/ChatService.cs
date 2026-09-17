using ChatService.Application.Data.Message.Request;
using ChatService.Application.Services;
using ChatService.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Infrastructure.Repositories;

public class ChatService : IChatService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IGroupChatMappingRepository _mappingRepository;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatService(IMessageRepository messageRepository, IConversationRepository conversationRepository,
        IGroupChatMappingRepository mappingRepository, IHubContext<ChatHub> hubContext)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _mappingRepository = mappingRepository;
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
        await _messageRepository.AddAsync(message);
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
                await _hubContext.Clients.Group(message.GroupId ?? request.ConversationId)
                    .SendAsync("MessageRecalled", message.Id, message.GroupId ?? request.ConversationId);
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
            // Phát reaction real-time cho mọi client trong hội thoại
            var msg = (await _messageRepository.GetByConversationIdAsync(new GetHistoryChatRequest(messageId, 0, 1))).FirstOrDefault();
            // Fallback: gửi theo messageId nếu không tìm thấy conversationId
            await _hubContext.Clients.All.SendAsync("MessageReaction", messageId, userId, emoji);
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
            await _hubContext.Clients.All.SendAsync("MessageRead", messageId, isRead);
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

    public async Task<Conversation> CreateOrGetGroupConversationAsync(string groupId, string userId, string? groupName = null, List<string>? memberIds = null)
    {
        var existing = await _mappingRepository.GetByGroupIdAsync(groupId);
        if (existing != null)
        {
            var conv = await _conversationRepository.GetByIdAsync(existing.ConversationId);
            if (conv != null) return conv;
        }

        var participants = memberIds != null && memberIds.Count > 0
            ? memberIds.Distinct().ToList()
            : new List<string> { userId };

        if (!participants.Contains(userId)) participants.Add(userId);

        var conversation = new Conversation
        {
            Id = Guid.NewGuid().ToString(),
            ParticipantIds = participants,
            CreatedAt = DateTime.UtcNow,
            IsGroup = true,
            GroupInfo = new Group
            {
                Name = groupName ?? $"Group {groupId[..Math.Min(8, groupId.Length)]}",
                AvatarUrl = string.Empty,
                AdminIds = new List<string> { userId },
                MemberIds = participants
            }
        };

        await _conversationRepository.AddAsync(conversation);
        await _mappingRepository.AddAsync(new GroupChatMapping
        {
            Id = Guid.NewGuid().ToString(),
            CommunityGroupId = groupId,
            ConversationId = conversation.Id,
            CreatedAt = DateTime.UtcNow
        });

        return conversation;
    }

    public async Task<Conversation> SyncGroupMembersAsync(string groupId, List<string> memberIds)
    {
        var mapping = await _mappingRepository.GetByGroupIdAsync(groupId);
        if (mapping == null) throw new InvalidOperationException($"No chat mapping for group {groupId}. Create conversation first.");

        var conv = await _conversationRepository.GetByIdAsync(mapping.ConversationId);
        if (conv == null) throw new InvalidOperationException("Conversation not found.");

        var distinct = memberIds.Distinct().ToList();
        conv.ParticipantIds = distinct;
        conv.GroupInfo.MemberIds = distinct;
        // Keep AdminIds as subset of members
        conv.GroupInfo.AdminIds = conv.GroupInfo.AdminIds.Where(distinct.Contains).ToList();
        await _conversationRepository.UpdateAsync(conv);
        return conv;
    }
}

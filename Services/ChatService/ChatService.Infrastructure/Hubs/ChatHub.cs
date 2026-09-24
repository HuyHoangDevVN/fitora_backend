using BuildingBlocks.Security;
using ChatService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Infrastructure.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IAuthorizeExtension _authorizeExtension;
        private readonly IPresenceService _presenceService;

        public ChatHub(IChatService chatService, IAuthorizeExtension authorizeExtension, IPresenceService presenceService)
        {
            _chatService = chatService;
            _authorizeExtension = authorizeExtension;
            _presenceService = presenceService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                try
                {
                    await _presenceService.SetOnlineAsync(userId);
                    await Clients.All.SendAsync("UserOnline", userId);
                }
                catch { /* Redis unavailable — presence is best-effort */ }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                try
                {
                    await _presenceService.SetOfflineAsync(userId);
                    var map = await _presenceService.GetPresenceAsync(new[] { userId });
                    var lastSeen = map.TryGetValue(userId, out var dto) ? dto.LastSeenAt : DateTime.UtcNow;
                    await Clients.All.SendAsync("UserOffline", userId, lastSeen);
                }
                catch { }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        }

        public async Task LeaveConversation(string conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
        }

        public async Task SendMessage(string conversationId, string content, string type)
        {
            var userId = _authorizeExtension.GetUserFromClaimToken().Id!.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated.");
            }

            var message = await _chatService.SendMessageAsync(userId, conversationId, content, type);

            await Clients.Group(conversationId).SendAsync(
                "ReceiveMessage",
                message.Id,
                message.SenderId,
                conversationId,
                message.Content,
                message.Type,
                message.Timestamp
            );
        }
    }
}

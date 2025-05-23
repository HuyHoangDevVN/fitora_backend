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

        public ChatHub(IChatService chatService, IAuthorizeExtension authorizeExtension)
        {
            _chatService = chatService;
            _authorizeExtension = authorizeExtension;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
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

            // Lưu tin nhắn vào MongoDB thông qua IChatService
            await _chatService.SendMessageAsync(userId, conversationId, content, type);

            // Gửi tin nhắn đến tất cả client trong conversation
            await Clients.Group(conversationId).SendAsync("ReceiveMessage", userId, conversationId, content, type);
        }
    }
}
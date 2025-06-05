using BuildingBlocks.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Application.DTOs.Notification.Requests;
using NotificationService.Application.Services.IServices;

namespace NotificationService.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IAuthorizeExtension _authorizeExtension;

        public NotificationHub(INotificationRepository notificationRepo, IAuthorizeExtension authorizeExtension)
        {
            _notificationRepo = notificationRepo;
            _authorizeExtension = authorizeExtension;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = _authorizeExtension.GetUserFromClaimToken().Id.ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = _authorizeExtension.GetUserFromClaimToken().Id.ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(Guid toUserId, string title, string content, Guid? objectId, int typeId)
        {
            var senderId = _authorizeExtension.GetUserFromClaimToken().Id;
            if (senderId == Guid.Empty)
                throw new HubException("User not authenticated.");

            var notification = await _notificationRepo.CreateAndReturnAsync(
                new CreateNotificationRequest(
                    toUserId,
                    senderId,
                    typeId,
                    objectId,
                    content,
                    title,
                    Domain.Enums.NotificationChannel.Web
                )
            );

            await Clients.Group(toUserId.ToString()).SendAsync(
                "ReceiveNotification",
                notification.Id,
                notification.SenderId,
                notification.UserId,
                notification.NotificationTypeId,
                notification.Content,
                notification.IsRead,
                notification.Channel
            );
        }

        public async Task MarkAllAsRead()
        {
            var user = _authorizeExtension.GetUserFromClaimToken();
            if (user == null || user.Id == Guid.Empty)
                throw new HubException("User not authenticated.");

            await _notificationRepo.MarkAllAsReadAsync(user.Id);
            await Clients.Group(user.Id.ToString()).SendAsync("AllNotificationsRead");
        }
    }
}
using BuildingBlocks.DTOs;
using UserService.Application.DTOs.RabbitMQ.Requests;
using UserService.Application.Services.IServices;
using UserService.Domain.Abstractions;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.Friendship.Commands.CreateFriendRequest;

public class CreateFriendRequestHandler(
    IFriendshipRepository friendshipRepo,
    IUserRepository userRepository,
    IRabbitMqPublisher<NotificationMessageDto> notificationPublisher,
    IMapper mapper)
    : ICommandHandler<CreateFriendRequestCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(CreateFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var result = await friendshipRepo.CreateFriendRequestAsync(request.Request);

        if (result.IsSuccess && request.Request.receiverId.HasValue)
        {
            await PublishFriendRequestNotificationAsync(request.Request.senderId, request.Request.receiverId.Value);
        }

        return result;
    }

    private async Task PublishFriendRequestNotificationAsync(Guid senderId, Guid receiverId)
    {
        try
        {
            var senders = await userRepository.GetUsersByIdsAsync(null, new List<Guid> { senderId });
            var senderName = senders.FirstOrDefault()?.Username ?? "Một người dùng";

            var message = new NotificationMessageDto
            {
                UserId = receiverId,
                SenderId = senderId,
                NotificationTypeId = 3, // friend_request
                ObjectId = senderId,
                Title = "Lời mời kết bạn",
                Content = $"{senderName} đã gửi cho bạn một lời mời kết bạn!",
                Channel = NotificationChannel.Web
            };
            await notificationPublisher.PublishMessageAsync(message, "noti_queue");
        }
        catch
        {
            // Không để lỗi gửi thông báo làm fail thao tác gửi lời mời chính.
        }
    }
}
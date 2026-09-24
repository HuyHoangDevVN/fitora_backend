using BuildingBlocks.DTOs;
using UserService.Application.DTOs.RabbitMQ.Requests;
using UserService.Application.Services.IServices;
using UserService.Domain.Abstractions;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.Friendship.Commands.AccpectFriendRequest;

public class AcceptFriendRequestHandler(
    IFriendshipRepository friendshipRepo,
    IUserRepository userRepository,
    IRabbitMqPublisher<NotificationMessageDto> notificationPublisher,
    IMapper mapper) : ICommandHandler<AcceptFriendRequestCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(AcceptFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var isSuccess = await friendshipRepo.AcceptFriendRequestAsync(request.request);

        if (isSuccess && request.request.receiverId.HasValue)
        {
            await PublishFriendAcceptedNotificationAsync(request.request.senderId, request.request.receiverId.Value);
        }

        return new ResponseDto(IsSuccess: isSuccess, Message: isSuccess ? "Accpect Success" : "Accpect Failed");
    }

    private async Task PublishFriendAcceptedNotificationAsync(Guid senderId, Guid receiverId)
    {
        try
        {
            var receivers = await userRepository.GetUsersByIdsAsync(null, new List<Guid> { receiverId });
            var receiverName = receivers.FirstOrDefault()?.Username ?? "Một người dùng";

            var message = new NotificationMessageDto
            {
                UserId = senderId,
                SenderId = receiverId,
                NotificationTypeId = 4, // friend_request_accepted
                ObjectId = receiverId,
                Title = "Lời mời kết bạn được chấp nhận",
                Content = $"{receiverName} đã chấp nhận lời mời kết bạn của bạn!",
                Channel = NotificationChannel.Web
            };
            await notificationPublisher.PublishMessageAsync(message, "noti_queue");
        }
        catch
        {
            // Không để lỗi gửi thông báo làm fail thao tác chấp nhận chính.
        }
    }
}
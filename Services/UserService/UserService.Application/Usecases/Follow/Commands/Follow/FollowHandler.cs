using BuildingBlocks.DTOs;
using UserService.Application.DTOs.RabbitMQ.Requests;
using UserService.Application.Services.IServices;
using UserService.Domain.Abstractions;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.Follow.Commands.Follow;

public class FollowHandler(
    IFollowRepository followRepository,
    IUserRepository userRepository,
    IRabbitMqPublisher<NotificationMessageDto> notificationPublisher,
    IMapper mapper)
    : ICommandHandler<FollowCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(FollowCommand request, CancellationToken cancellationToken)
    {
        var result = await followRepository.FollowAsync(request.Request);

        if (result.IsSuccess)
        {
            await PublishFollowNotificationAsync(request.Request, cancellationToken);
        }

        return result;
    }

    private async Task PublishFollowNotificationAsync(DTOs.Follow.Requests.FollowRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var followers = await userRepository.GetUsersByIdsAsync(null, new List<Guid> { request.FollowerId });
            var followerName = followers.FirstOrDefault()?.Username ?? "Một người dùng";

            var message = new NotificationMessageDto
            {
                UserId = request.FollowedId,
                SenderId = request.FollowerId,
                NotificationTypeId = 2, // follow
                ObjectId = request.FollowerId,
                Title = "Người theo dõi mới",
                Content = $"{followerName} đã bắt đầu theo dõi bạn!",
                Channel = NotificationChannel.Web
            };
            await notificationPublisher.PublishMessageAsync(message, "noti_queue");
        }
        catch
        {
            // Không để lỗi gửi thông báo làm fail thao tác follow chính.
        }
    }
}
using BuildingBlocks.DTOs;

namespace NotificationService.Application.Usecases.NotificationType.Commands.Update;

public class UpdateNotiTypeHandler(INotificationTypeRepository notificationTypeRepo, IMapper mapper) : ICommandHandler<UpdateNotiTypeCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(UpdateNotiTypeCommand request, CancellationToken cancellationToken)
    {
        var notificationType = mapper.Map<Domain.Models.NotificationType>(request.Request);
        var result = await notificationTypeRepo.UpdateAsync(notificationType);
        return new ResponseDto(null, result, result ? "Notification type updated successfully." : "Failed to update notification type.");
    }
}
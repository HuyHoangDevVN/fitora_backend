using BuildingBlocks.DTOs;

namespace NotificationService.Application.Usecases.NotificationType.Commands.Delete;

public class DeleteNotiTypeHandler(INotificationTypeRepository notificationTypeRepo) : ICommandHandler<DeleteNotiTypeCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(DeleteNotiTypeCommand request, CancellationToken cancellationToken)
    {
        var result = await notificationTypeRepo.DeleteAsync(request.Id);
        return new ResponseDto(null, result, result ? "Notification type deleted successfully." : "Failed to delete notification type.");
    }
}
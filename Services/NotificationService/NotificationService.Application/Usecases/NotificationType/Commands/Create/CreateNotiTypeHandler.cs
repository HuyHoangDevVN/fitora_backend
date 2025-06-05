using BuildingBlocks.DTOs;

namespace NotificationService.Application.Usecases.NotificationType.Commands.Create;

public class CreateNotiTypeHandler(INotificationTypeRepository notificationTypeRepo, IMapper mapper) : ICommandHandler<CreateNotiTypeCommand,ResponseDto>
{
    public async Task<ResponseDto> Handle(CreateNotiTypeCommand command, CancellationToken cancellationToken)
    {
        var notificationType = mapper.Map<Domain.Models.NotificationType>(command.Request);
        var result = await notificationTypeRepo.CreateAsync(notificationType);
        return new ResponseDto(null, result, result ? "Notification type created successfully." : "Failed to create notification type.");
    }
}
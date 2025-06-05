using BuildingBlocks.DTOs;
using NotificationService.Application.DTOs.Notification.Requests;

namespace NotificationService.Application.Usecases.NotificationType.Commands.Update;

public record UpdateNotiTypeCommand(UpdateNotificationRequest Request): ICommand<ResponseDto>;
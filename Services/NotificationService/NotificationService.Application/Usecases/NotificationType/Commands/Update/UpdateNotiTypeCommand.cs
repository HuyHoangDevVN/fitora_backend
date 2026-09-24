using BuildingBlocks.DTOs;
using NotificationService.Application.DTOs.NotificationType.Requests;

namespace NotificationService.Application.Usecases.NotificationType.Commands.Update;

public record UpdateNotiTypeCommand(UpdateNotificationTypeRequest Request): ICommand<ResponseDto>;
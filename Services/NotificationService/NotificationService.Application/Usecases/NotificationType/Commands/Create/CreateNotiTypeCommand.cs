using BuildingBlocks.DTOs;
using NotificationService.Application.DTOs.Notification.Requests;
using NotificationService.Application.DTOs.NotificationType.Requests;

namespace NotificationService.Application.Usecases.NotificationType.Commands.Create;

public record CreateNotiTypeCommand(CreateNotificationTypeRequest Request) : ICommand<ResponseDto>;
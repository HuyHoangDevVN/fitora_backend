using BuildingBlocks.DTOs;

namespace NotificationService.Application.Usecases.NotificationType.Commands.Delete;

public record DeleteNotiTypeCommand(int Id) : ICommand<ResponseDto>;
using AutoMapper;
using NotificationService.Application.DTOs.Notification.Responses;
using NotificationService.Application.DTOs.NotificationType.Requests;

namespace NotificationService.Application.Mapper;

public class ServiceProfile : Profile
{
    public ServiceProfile()
    {
        CreateMap<Notification, NotificationDto>().ReverseMap();

        CreateMap<CreateNotificationTypeRequest, NotificationType>().ReverseMap();
        CreateMap<UpdateNotificationTypeRequest, NotificationType>().ReverseMap();
    }
}
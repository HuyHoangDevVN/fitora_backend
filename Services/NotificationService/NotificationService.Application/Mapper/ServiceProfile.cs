using AutoMapper;
using NotificationService.Application.DTOs.Notification.Responses;

namespace NotificationService.Application.Mapper;

public class ServiceProfile : Profile
{
    public ServiceProfile()
    {
        CreateMap<Notification, NotificationDto>().ReverseMap();
    }
}

using InteractService.Domain.Enums;

namespace InteractService.Application.DTOs.RabbitMQ.Requests;

public class NotificationMessageDto
{
    public Guid UserId { get; set; }
    public Guid? SenderId { get; set; }
    public int NotificationTypeId { get; set; }
    public Guid? ObjectId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public NotificationChannel Channel { get; set; }
}
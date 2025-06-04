namespace NotificationService.Application.Data;

public interface IApplicationDbContext
{
    DbSet<Notification> Notifications { get; }
    DbSet<NotificationSetting> NotificationSettings { get; }
    DbSet<NotificationTemplate> NotificationTemplates { get; }
    DbSet<NotificationType> NotificationTypes { get; }

    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
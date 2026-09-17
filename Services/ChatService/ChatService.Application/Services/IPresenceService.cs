namespace ChatService.Application.Services;

public record PresenceDto(bool IsOnline, DateTime? LastSeenAt);

public interface IPresenceService
{
    Task SetOnlineAsync(string userId);
    Task SetOfflineAsync(string userId);
    Task<Dictionary<string, PresenceDto>> GetPresenceAsync(IEnumerable<string> userIds);
}

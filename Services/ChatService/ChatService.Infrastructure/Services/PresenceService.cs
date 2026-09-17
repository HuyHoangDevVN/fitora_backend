using System.Text.Json;
using ChatService.Application.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace ChatService.Infrastructure.Services;

public class PresenceService : IPresenceService
{
    private readonly IDistributedCache _cache;
    private static string Key(string userId) => $"presence:{userId}";

    public PresenceService(IDistributedCache cache) => _cache = cache;

    public async Task SetOnlineAsync(string userId)
    {
        var payload = JsonSerializer.Serialize(new { isOnline = true, lastSeenAt = DateTime.UtcNow });
        await _cache.SetStringAsync(Key(userId), payload, new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(5)
        });
    }

    public async Task SetOfflineAsync(string userId)
    {
        var payload = JsonSerializer.Serialize(new { isOnline = false, lastSeenAt = DateTime.UtcNow });
        await _cache.SetStringAsync(Key(userId), payload);
    }

    public async Task<Dictionary<string, PresenceDto>> GetPresenceAsync(IEnumerable<string> userIds)
    {
        var result = new Dictionary<string, PresenceDto>();
        foreach (var uid in userIds)
        {
            if (string.IsNullOrWhiteSpace(uid)) continue;
            var raw = await _cache.GetStringAsync(Key(uid));
            if (raw is null)
            {
                result[uid] = new PresenceDto(false, null);
                continue;
            }
            try
            {
                using var doc = JsonDocument.Parse(raw);
                var isOnline = doc.RootElement.TryGetProperty("isOnline", out var a) && a.GetBoolean();
                DateTime? lastSeen = null;
                if (doc.RootElement.TryGetProperty("lastSeenAt", out var b) && b.TryGetDateTime(out var dt))
                    lastSeen = dt;
                result[uid] = new PresenceDto(isOnline, lastSeen);
            }
            catch
            {
                result[uid] = new PresenceDto(false, null);
            }
        }
        return result;
    }
}

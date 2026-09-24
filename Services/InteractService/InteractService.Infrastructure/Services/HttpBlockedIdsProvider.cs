using System.Text.Json;
using InteractService.Application.Services.IServices;
using Microsoft.Extensions.Logging;

namespace InteractService.Infrastructure.Services;

public class HttpBlockedIdsProvider : IBlockedIdsProvider
{
    private readonly IHttpClientFactory _factory;
    private readonly ILogger<HttpBlockedIdsProvider> _logger;

    public HttpBlockedIdsProvider(IHttpClientFactory factory, ILogger<HttpBlockedIdsProvider> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<IReadOnlySet<Guid>> GetBlockedUserIdsAsync(Guid currentUserId, CancellationToken ct = default)
    {
        try
        {
            var client = _factory.CreateClient("UserService");
            var resp = await client.GetAsync("api/user/block/blocked-ids", ct);
            if (!resp.IsSuccessStatusCode) return new HashSet<Guid>();
            return ParseIds(await resp.Content.ReadAsStringAsync(ct), "blockedUserIds");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "HttpBlockedIdsProvider: failed to get blocked user ids for {UserId}", currentUserId);
            return new HashSet<Guid>();
        }
    }

    public async Task<IReadOnlySet<Guid>> GetBlockedGroupIdsAsync(Guid currentUserId, CancellationToken ct = default)
    {
        try
        {
            var client = _factory.CreateClient("UserService");
            var resp = await client.GetAsync("api/user/block/blocked-ids", ct);
            if (!resp.IsSuccessStatusCode) return new HashSet<Guid>();
            return ParseIds(await resp.Content.ReadAsStringAsync(ct), "blockedGroupIds");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "HttpBlockedIdsProvider: failed to get blocked group ids for {UserId}", currentUserId);
            return new HashSet<Guid>();
        }
    }

    private static HashSet<Guid> ParseIds(string json, string key)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        JsonElement arr = default;
        bool found = false;
        if (root.TryGetProperty(key, out arr)) found = true;
        else if (root.TryGetProperty(char.ToUpper(key[0]) + key[1..], out arr)) found = true;
        else if (root.TryGetProperty("data", out var d) && d.TryGetProperty(key, out arr)) found = true;
        else if (root.TryGetProperty("Data", out var d2) && d2.TryGetProperty(char.ToUpper(key[0]) + key[1..], out arr)) found = true;
        if (!found) return new HashSet<Guid>();
        var set = new HashSet<Guid>();
        foreach (var el in arr.EnumerateArray())
            if (Guid.TryParse(el.GetString(), out var g)) set.Add(g);
        return set;
    }
}

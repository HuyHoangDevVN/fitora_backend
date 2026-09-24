using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace BuildingBlocks.HealthChecks;

public class RedisHealthCheck(IConnectionMultiplexer redis) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = redis.GetDatabase();
            var pong = await db.PingAsync();
            return HealthCheckResult.Healthy($"Redis connection is healthy (latency: {pong.TotalMilliseconds}ms)");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis connection failed", ex);
        }
    }
}

using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace ChatService.Infrastructure.HealthChecks;

public class MongoDbHealthCheck(IMongoDatabase database) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await database.RunCommandAsync((Command<MongoDB.Bson.BsonDocument>)"{ping:1}", cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy("MongoDB connection is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("MongoDB connection failed", ex);
        }
    }
}

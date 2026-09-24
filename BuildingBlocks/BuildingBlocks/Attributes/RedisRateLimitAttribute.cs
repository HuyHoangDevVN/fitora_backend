using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BuildingBlocks.Attributes;

public class RedisRateLimitAttribute : Attribute, IAsyncActionFilter, IEndpointFilter
{
    private readonly int _limit;
    private readonly TimeSpan _period;
    private readonly string _keyPrefix;

    public RedisRateLimitAttribute(int limit, int periodSeconds = 60, string? keyPrefix = null)
    {
        _limit = limit;
        _period = TimeSpan.FromSeconds(periodSeconds);
        _keyPrefix = keyPrefix ?? "ratelimit";
    }


    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var redis = context.HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();
        var actionName = context.ActionDescriptor.DisplayName;
        var redisKey = $"{_keyPrefix}:{actionName}:{clientIp}";

        var db = redis.GetDatabase();
        var currentCount = await db.StringIncrementAsync(redisKey);

        if (currentCount == 1)
        {
            await db.KeyExpireAsync(redisKey, _period);
        }

        if (currentCount > _limit)
        {
            context.Result = new ContentResult
            {
                StatusCode = 429,
                Content = "Rate limit exceeded, Try again later"
            };
            return;
        }

        await next();
    }

    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        throw new NotImplementedException();
    }
}
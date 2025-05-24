using System.Reflection;
using BuildingBlocks.Abstractions;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Security;
using ChatService.Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace ChatService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        var jwtToken = new JwtConfiguration();
        configuration.GetSection("JwtOptions").Bind(jwtToken);

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMqSettings"));
        services.AddScoped<IAuthorizeExtension, AuthorizeExtension>();


        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });
        services.AddFeatureManagement();
        services.AddHttpContextAccessor();
        services.AddAutoMapper(typeof(ServiceProfile));
        return services;
    }
}
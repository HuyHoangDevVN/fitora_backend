using System.Reflection;
using System.Text;
using BuildingBlocks.Abstractions;
using BuildingBlocks.Behaviors;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Mapper;
using UserService.Application.Messaging;
using UserService.Application.Messaging.MessageHandlers;
using UserService.Application.Messaging.MessageHandlers.IHandlers;
using UserService.Application.Services;
using UserService.Domain.Abstractions;

namespace UserService.Application;

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
        
        
        services.AddScoped(typeof(IRabbitMqPublisher<>), typeof(RabbitMqPublisher<>));
        
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMqSettings"));
        services.AddSingleton<IRabbitMqConsumer<UserRegisteredMessageDto>, RabbitMqConsumer<UserRegisteredMessageDto>>();
        services.AddScoped<IMessageHandler<UserRegisteredMessageDto>, UserRegisteredMessageHandler>();
        // services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthorizeExtension, AuthorizeExtension>();
        services.AddHostedService<RabbitMqConsumerHostedService>();

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
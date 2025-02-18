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
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFriendshipRepository, FriendshipRepository>();
        services.AddScoped<IAuthorizeExtension, AuthorizeExtension>();
        
        services.AddHostedService<RabbitMqConsumerHostedService>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });
        services.AddFeatureManagement();
        services.AddHttpContextAccessor();
        services.AddAutoMapper(typeof(ServiceProfile));
        services.AddApplicationAuthentication(configuration);
        return services;
    }

    private static IServiceCollection AddApplicationAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection("JwtOptions");
        var secret = jwtOptions["Secret"]!;
        var audience = jwtOptions["Audience"]!;
        var issuer = jwtOptions["Issuer"]!;
        
        var key = Encoding.UTF8.GetBytes(secret);

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidIssuer = issuer,
                ClockSkew = TimeSpan.Zero
            };
        });

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidAudience = audience,
            ValidIssuer = issuer,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
        services.AddSingleton(tokenValidationParameters);
        services.AddAuthorization();
        return services;
    }
    
    
}
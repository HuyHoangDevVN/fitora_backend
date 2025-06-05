using System.Text;
using BuildingBlocks.RepositoryBase.EntityFramework;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NotificationService.Application.Data;
using NotificationService.Application.DTOs.MessegeQueue.Notification;
using NotificationService.Application.Helpers;
using NotificationService.Application.Mapper;
using NotificationService.Application.Messaging;
using NotificationService.Application.Messaging.MessageHandlers;
using NotificationService.Application.Messaging.MessageHandlers.IHandlers;
using NotificationService.Application.Services;
using NotificationService.Application.Services.IServices;
using NotificationService.Domain.Abstractions;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Repositories;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        services.AddTransient(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });

        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped(typeof(IRabbitMqPublisher<>), typeof(RabbitMqPublisher<>));
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMqSettings"));
        
        services.AddSingleton<IRabbitMqPublisher<NotificationMessageDto>, RabbitMqPublisher<NotificationMessageDto>>();
        services.AddSingleton<IRabbitMqConsumer<NotificationMessageDto>, RabbitMqConsumer<NotificationMessageDto>>();
        services.AddScoped<IMessageHandler<NotificationMessageDto>, NotificationMessageHandler>();
        services.AddHostedService<NotificationConsumerHostedService>();
        services.AddHostedService<SignalRNotificationConsumerHostedService>();

        services.AddAutoMapper(typeof(ServiceProfile));
        return services;
    }

    public static IServiceCollection AddApplicationAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection("ApiSettings:JwtOptions");
        var secret = jwtOptions["Secret"]!;
        var audience = jwtOptions["Audience"]!;
        var issuer = jwtOptions["Issuer"]!;

        var key = Encoding.UTF8.GetBytes(secret);

        services.Configure<JwtOptionsSetting>(configuration.GetSection("ApiSettings:JwtOptions"));

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Đọc accessToken từ Cookie nếu có
                        if (context.Request.Cookies.ContainsKey("accessToken"))
                        {
                            context.Token = context.Request.Cookies["accessToken"];
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
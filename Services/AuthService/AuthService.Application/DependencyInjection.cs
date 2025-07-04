﻿using System.Reflection;
using System.Text;
using AuthService.Application.Auths.Commands.AuthLogin;
using AuthService.Application.Messaging;
using AuthService.Application.Services;
using AuthService.Domain.Abstractions;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Security;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });


        services.AddValidatorsFromAssemblyContaining<AuthLoginCommandValidator>();

        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMQ"));
        services.AddScoped(typeof(IRabbitMqPublisher<>), typeof(RabbitMqPublisher<>));
        services.AddScoped(typeof(IRabbitMqConsumer<>), typeof(RabbitMqConsumer<>));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });
        services.AddFeatureManagement();
        services.AddHttpContextAccessor();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IKeyRepository<Guid>, KeyRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IAuthorizeExtension, AuthorizeExtension>();
        //services.Decorate<IAuthRepository, TokenManagementRepository>();
        services.AddAutoMapper(typeof(ServiceProfile));
        return services;
    }

    public static IServiceCollection AddApplicationAuthentication(this IServiceCollection services, IConfiguration configuration)
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
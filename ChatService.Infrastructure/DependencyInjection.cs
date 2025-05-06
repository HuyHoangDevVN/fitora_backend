using ChatService.Application.Services;
using ChatService.Infrastructure.Data.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var database = new MongoDbConfiguration().GetDatabase(
            configuration["MongoDb:ConnectionString"],
            configuration["MongoDb:Database"]);
        services.AddSingleton(database); // Đăng ký IMongoDatabase
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        return services;
    }
}
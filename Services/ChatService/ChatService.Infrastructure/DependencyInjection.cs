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
        string? connectionString = configuration["MongoDb:ConnectionString"];
        string? databaseName = configuration["MongoDb:Database"];

        // Khởi tạo IMongoDatabase
        var mongoConfig = new MongoDbConfiguration();
        var database = mongoConfig.GetDatabase(connectionString, databaseName);

        services.AddSingleton(database);
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IChatService, Repositories.ChatService>();
        return services;
    }
}
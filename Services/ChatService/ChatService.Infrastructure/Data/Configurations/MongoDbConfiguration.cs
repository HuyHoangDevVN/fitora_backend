using MongoDB.Driver;

namespace ChatService.Infrastructure.Data.Configurations
{
    public class MongoDbConfiguration
    {
        public IMongoDatabase GetDatabase(string? connectionString, string? databaseName)
        {
            var client = new MongoClient(connectionString);
            return client.GetDatabase(databaseName);
        }
    }
}
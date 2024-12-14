using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using NotificationApi.Models;

namespace NotificationApi.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
            _database = client.GetDatabase(configuration["MongoDB:Database"]);

            var collection = _database.GetCollection<User>("Users");
            var indexKeysDefinition = Builders<User>.IndexKeys.Ascending(u => u.Email);
            collection.Indexes.CreateOne(new CreateIndexModel<User>(indexKeysDefinition));
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    }
}

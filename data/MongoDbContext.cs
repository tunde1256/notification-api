using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NotificationApi.Models;

namespace NotificationApi.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoDbContext> _logger;

        public MongoDbContext(IConfiguration configuration, ILogger<MongoDbContext> logger)
        {
            _logger = logger;

            var connectionString = configuration["MongoDB:ConnectionString"];
            _logger.LogInformation("Initializing MongoDB connection to: {ConnectionString}", connectionString);

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(configuration["MongoDB:Database"]);

            var databaseName = _database.DatabaseNamespace.DatabaseName;
            _logger.LogInformation("Connected to MongoDB database: {DatabaseName}", databaseName);

            var collection = _database.GetCollection<User>("Users");
            var indexKeysDefinition = Builders<User>.IndexKeys.Ascending(u => u.Email);
            collection.Indexes.CreateOne(new CreateIndexModel<User>(indexKeysDefinition));

            _logger.LogInformation("Index created on 'Email' field in 'Users' collection.");
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    }
}

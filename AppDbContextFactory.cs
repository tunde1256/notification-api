using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design; // Add this
using Microsoft.Extensions.Logging;
using NotificationApi.Data;

namespace NotificationApi
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Provide configuration and logger
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<AppDbContext>();

            optionsBuilder.UseSqlServer("YourConnectionStringHere");

            return new AppDbContext(optionsBuilder.Options, logger);
        }
    }
}

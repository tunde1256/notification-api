using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationApi.Models;

namespace NotificationApi.Data
{
    public class AppDbContext : DbContext
    {
        private readonly ILogger<AppDbContext> _logger;

        public AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger) : base(options)
        {
            _logger = logger;
        }

        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole())) 
                          .EnableSensitiveDataLogging()  
                          .EnableDetailedErrors();  
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            _logger.LogInformation("OnModelCreating called for AppDbContext.");
           
        }
    }
}

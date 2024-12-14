using MongoDB.Driver;
using NotificationApi.Data;
using NotificationApi.Models;  // Ensure you have this to reference the User class
using System.Threading.Tasks;

namespace NotificationApi.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;

        // Constructor
        public UserService(MongoDbContext mongoDbContext, IEmailService emailService, ISmsService smsService)
        {
            _users = mongoDbContext.Users;
            _emailService = emailService;
            _smsService = smsService;
        }

        // Corrected method to match IUserService signature
        public async Task<User> GetUserByIdAsync(int userId)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            return user; // Make sure you're returning Task<User>
        }

        // Corrected method to match IUserService signature
        public async Task SendNotificationAsync(User user, string message, string notificationType)
        {
            if (user == null) 
                throw new ArgumentNullException(nameof(user), "User cannot be null.");

            if (string.IsNullOrEmpty(notificationType))
                throw new ArgumentException("Notification type cannot be null or empty.", nameof(notificationType));

            // Handle different notification types
            if (notificationType.ToLower() == "email")
            {
                var subject = "Notification from Our Service";
                var body = message;

                // Sending the email asynchronously
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            else if (notificationType.ToLower() == "sms")
            {
                // Ensure user model has PhoneNumber property
                var phoneNumber = user.PhoneNumber; 
                
                if (string.IsNullOrEmpty(phoneNumber))
                    throw new ArgumentException("User does not have a valid phone number.");

                // Sending SMS asynchronously
                await _smsService.SendSmsAsync(phoneNumber, message);
            }
            else
            {
                throw new ArgumentException("Invalid notification type. Supported types: email, sms.", nameof(notificationType));
            }
        }
    }
}

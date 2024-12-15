using MongoDB.Driver;
using NotificationApi.Data;
using NotificationApi.Models;  
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NotificationApi.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<UserService> _logger;

        
        public UserService(MongoDbContext mongoDbContext, IEmailService emailService, ISmsService smsService, ILogger<UserService> logger)
        {
            _users = mongoDbContext.Users;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;

            _logger.LogInformation("UserService initialized.");
        }

        
        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Fetching user with ID: {UserId}", userId);

                var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", userId);
                }
                else
                {
                    _logger.LogInformation("User with ID {UserId} fetched successfully.", userId);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user with ID: {UserId}", userId);
                throw;
            }
        }

        public async Task SendNotificationAsync(User user, string message, string notificationType)
        {
            try
            {
                if (user == null)
                {
                    _logger.LogError("User is null. Cannot send notification.");
                    throw new ArgumentNullException(nameof(user), "User cannot be null.");
                }

                if (string.IsNullOrEmpty(notificationType))
                {
                    _logger.LogError("Notification type is null or empty. Cannot send notification.");
                    throw new ArgumentException("Notification type cannot be null or empty.", nameof(notificationType));
                }

                _logger.LogInformation("Sending {NotificationType} notification to user: {UserId}", notificationType, user.Id);

                if (notificationType.ToLower() == "email")
                {
                    var subject = "Notification from Our Service";
                    var body = message;

                    _logger.LogInformation("Sending email to {Email}", user.Email);
                    await _emailService.SendEmailAsync(user.Email, subject, body);
                    _logger.LogInformation("Email sent successfully to {Email}", user.Email);
                }
                else if (notificationType.ToLower() == "sms")
                {
                    var phoneNumber = user.PhoneNumber;

                    if (string.IsNullOrEmpty(phoneNumber))
                    {
                        _logger.LogError("User {UserId} does not have a valid phone number.", user.Id);
                        throw new ArgumentException("User does not have a valid phone number.");
                    }

                    _logger.LogInformation("Sending SMS to phone number: {PhoneNumber}", phoneNumber);
                    await _smsService.SendSmsAsync(phoneNumber, message);
                    _logger.LogInformation("SMS sent successfully to phone number: {PhoneNumber}", phoneNumber);
                }
                else
                {
                    _logger.LogError("Invalid notification type: {NotificationType}. Supported types are: email, sms.", notificationType);
                    throw new ArgumentException("Invalid notification type. Supported types: email, sms.", nameof(notificationType));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending {NotificationType} notification to user: {UserId}", notificationType, user?.Id);
                throw;
            }
        }
    }
}

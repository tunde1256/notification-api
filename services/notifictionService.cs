using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NotificationApi.Models; 

namespace NotificationApi.Services
{
    public class NotificationService
    {
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IEmailService emailService, ISmsService smsService, ILogger<NotificationService> logger)
        {
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;

            _logger.LogInformation("NotificationService initialized.");
        }

        public async Task SendNotificationAsync(User user, string message)
        {
            if (user == null)
            {
                _logger.LogError("User is null. Cannot send notification.");
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            if (string.IsNullOrEmpty(message))
            {
                _logger.LogError("Message is null or empty. Cannot send notification.");
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));
            }

            _logger.LogInformation("Preparing to send notification to User: {UserId}, Notification Preference: {Preference}", 
                                    user.Id, user.NotificationPreference);

            try
            {
                if (user.NotificationPreference.Equals("Email", StringComparison.OrdinalIgnoreCase))
                {
                    string subject = "Notification";
                    string body = message;

                    _logger.LogInformation("Sending email to {Email}", user.Email);

                    await _emailService.SendEmailAsync(user.Email, subject, body);

                    _logger.LogInformation("Email sent successfully to {Email}", user.Email);
                }
                else if (user.NotificationPreference.Equals("SMS", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Sending SMS to {PhoneNumber}", user.PhoneNumber);

                    await _smsService.SendSmsAsync(user.PhoneNumber, message);

                    _logger.LogInformation("SMS sent successfully to {PhoneNumber}", user.PhoneNumber);
                }
                else
                {
                    _logger.LogWarning("Invalid notification preference: {Preference} for User: {UserId}", 
                                        user.NotificationPreference, user.Id);
                    throw new ArgumentException("Invalid notification preference", nameof(user.NotificationPreference));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending notification to User: {UserId}", user.Id);
                throw;
            }
        }
    }
}

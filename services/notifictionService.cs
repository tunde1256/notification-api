using System;
using System.Threading.Tasks;
using NotificationApi.Models; // Ensure this is the correct namespace

namespace NotificationApi.Services
{
    public class NotificationService
    {
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;

        public NotificationService(IEmailService emailService, ISmsService smsService)
        {
            _emailService = emailService;
            _smsService = smsService;
        }

        public async Task SendNotificationAsync(User user, string message)
        {
            if (user.NotificationPreference.Equals("Email", StringComparison.OrdinalIgnoreCase))
            {
                // Add a subject and body for the email
                string subject = "Notification";
                string body = message; // You can customize this as needed

                // Fix the method call by including the third argument
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            else if (user.NotificationPreference.Equals("SMS", StringComparison.OrdinalIgnoreCase))
            {
                await _smsService.SendSmsAsync(user.PhoneNumber, message);
            }
            else
            {
                throw new ArgumentException("Invalid notification preference");
            }
        }
    }
}

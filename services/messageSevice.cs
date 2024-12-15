using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NotificationApi.Services
{
    public class MessageService  
    {
        private readonly ISmsService _smsService;
        private readonly IEmailService _emailService;
        private readonly ILogger<MessageService> _logger;

        public MessageService(ISmsService smsService, IEmailService emailService, ILogger<MessageService> logger)
        {
            _smsService = smsService;
            _emailService = emailService;
            _logger = logger;

            _logger.LogInformation("MessageService initialized.");
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                _logger.LogError("Phone number is null or empty. Cannot send SMS.");
                throw new ArgumentException("Phone number cannot be null or empty.", nameof(phoneNumber));
            }

            if (string.IsNullOrEmpty(message))
            {
                _logger.LogError("Message is null or empty. Cannot send SMS.");
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));
            }

            _logger.LogInformation("Attempting to send SMS to {PhoneNumber}", phoneNumber);

            try
            {
                await _smsService.SendSmsAsync(phoneNumber, message);
                _logger.LogInformation("SMS successfully sent to {PhoneNumber}", phoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending SMS to {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            if (string.IsNullOrEmpty(recipientEmail))
            {
                _logger.LogError("Recipient email is null or empty. Cannot send email.");
                throw new ArgumentException("Recipient email cannot be null or empty.", nameof(recipientEmail));
            }

            if (string.IsNullOrEmpty(subject))
            {
                _logger.LogError("Subject is null or empty. Cannot send email.");
                throw new ArgumentException("Subject cannot be null or empty.", nameof(subject));
            }

            if (string.IsNullOrEmpty(body))
            {
                _logger.LogError("Email body is null or empty. Cannot send email.");
                throw new ArgumentException("Email body cannot be null or empty.", nameof(body));
            }

            _logger.LogInformation("Attempting to send email to {RecipientEmail}", recipientEmail);

            try
            {
                await _emailService.SendEmailAsync(recipientEmail, subject, body);
                _logger.LogInformation("Email successfully sent to {RecipientEmail}", recipientEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending email to {RecipientEmail}", recipientEmail);
                throw;
            }
        }
    }
}

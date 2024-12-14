using NotificationApi.Services;

namespace NotificationApi.Services
{
    public class MessageService  // This could be your general service that calls ISmsService and IEmailService
    {
        private readonly ISmsService _smsService;
        private readonly IEmailService _emailService;

        public MessageService(ISmsService smsService, IEmailService emailService)
        {
            _smsService = smsService;
            _emailService = emailService;
        }

        public Task SendSmsAsync(string phoneNumber, string message)
        {
            return _smsService.SendSmsAsync(phoneNumber, message);
        }

        public Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            return _emailService.SendEmailAsync(recipientEmail, subject, body);
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace NotificationApi.Services
{
    public class SmsService : ISmsService
    {
        private readonly string _twilioAccountSid;
        private readonly string _twilioAuthToken;
        private readonly string _twilioPhoneNumber;
        private readonly ILogger<SmsService> _logger;

        public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
        {
            _twilioAccountSid = configuration["Twilio:AccountSid"];
            _twilioAuthToken = configuration["Twilio:AuthToken"];
            _twilioPhoneNumber = configuration["Twilio:FromPhoneNumber"];
            _logger = logger;

            _logger.LogInformation("SmsService initialized with TwilioPhoneNumber: {PhoneNumber}", _twilioPhoneNumber);
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            _logger.LogWarning("SendEmailAsync method was called, but this service does not handle emails.");
            throw new NotImplementedException("This service does not handle emails.");
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                _logger.LogInformation("Attempting to send SMS to {PhoneNumber}", phoneNumber);

                TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);

               
                var messageResource = await MessageResource.CreateAsync(
                    to: new PhoneNumber(phoneNumber),
                    from: new PhoneNumber(_twilioPhoneNumber),
                    body: message
                );

                _logger.LogInformation("SMS sent successfully to {PhoneNumber}. SID: {MessageSid}", phoneNumber, messageResource.Sid);
            }
            catch (Exception ex)
            {
                // Log any exception
                _logger.LogError(ex, "Error occurred while sending SMS to {PhoneNumber}", phoneNumber);
                throw;
            }
        }
    }
}

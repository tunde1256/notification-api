using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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

        public SmsService(IConfiguration configuration)
        {
            // Retrieve Twilio configuration values from appsettings.json
            _twilioAccountSid = configuration["Twilio:AccountSid"];
            _twilioAuthToken = configuration["Twilio:AuthToken"];
            _twilioPhoneNumber = configuration["Twilio:FromPhoneNumber"];
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            throw new NotImplementedException("This service does not handle emails.");
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                // Initialize Twilio client
                TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);

                // Send SMS
                var messageResource = await MessageResource.CreateAsync(
                    to: new PhoneNumber(phoneNumber),
                    from: new PhoneNumber(_twilioPhoneNumber),
                    body: message
                );

                // Log the response
                Console.WriteLine($"SMS sent successfully to {phoneNumber}. SID: {messageResource.Sid}");
            }
            catch (Exception ex)
            {
                // Log any exception
                Console.WriteLine($"Error sending SMS: {ex.Message}");
            }
        }
    }
}

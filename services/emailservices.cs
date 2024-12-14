using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly string _senderEmail;

        public EmailService(IConfiguration configuration)
        {
            // Load configuration values once
            _senderEmail = configuration["EmailSettings:SenderEmail"];
            var senderPassword = configuration["EmailSettings:SenderPassword"];
            var smtpHost = configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);

            // Initialize and configure SmtpClient once (reused across requests)
            _smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(_senderEmail, senderPassword),
                EnableSsl = true
            };
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var mailMessage = new MailMessage(_senderEmail, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            // Retry logic for transient failures
            const int maxRetries = 3;
            const int delayBetweenRetriesMs = 1000; // 1 second

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await _smtpClient.SendMailAsync(mailMessage);
                    return; // Email sent successfully, exit the loop
                }
                catch (SmtpException ex) when (attempt < maxRetries)
                {
                    // Log the exception (logging logic not shown here)
                    Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");
                    Thread.Sleep(delayBetweenRetriesMs); // Wait before retrying
                }
            }

            throw new Exception("Failed to send email after multiple attempts.");
        }
    }
}

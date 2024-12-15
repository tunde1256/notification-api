using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _senderEmail = configuration["EmailSettings:SenderEmail"];
            var senderPassword = configuration["EmailSettings:SenderPassword"];
            var smtpHost = configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);

            _smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(_senderEmail, senderPassword),
                EnableSsl = true
            };

            _logger = logger;
            _logger.LogInformation("EmailService initialized with SMTP host: {SmtpHost} and port: {SmtpPort}", smtpHost, smtpPort);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var mailMessage = new MailMessage(_senderEmail, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            _logger.LogInformation("Preparing to send email to: {ToEmail}, Subject: {Subject}", toEmail, subject);

          
            const int maxRetries = 3;
            const int delayBetweenRetriesMs = 1000; 

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("Attempt {Attempt} to send email to {ToEmail}", attempt, toEmail);
                    await _smtpClient.SendMailAsync(mailMessage);
                    _logger.LogInformation("Email successfully sent to {ToEmail}.", toEmail);
                    return; 
                          }
                catch (SmtpException ex) when (attempt < maxRetries)
                {
                    _logger.LogWarning(ex, "Attempt {Attempt} to send email to {ToEmail} failed: {ErrorMessage}", attempt, toEmail, ex.Message);
                    Thread.Sleep(delayBetweenRetriesMs); 
                }
            }

            _logger.LogError("Failed to send email to {ToEmail} after {MaxRetries} attempts.", toEmail, maxRetries);
            throw new Exception("Failed to send email after multiple attempts.");
        }
    }
}

using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

public class EmailService
{
    private readonly string _smtpHost = "smtp.gmail.com"; 
    private readonly int _smtpPort = 587;
    private readonly string _senderEmail = "medimapapplication@gmail.com"; 
    private readonly string _senderPassword = "rbgk ijja flam cjzi";  

    public async Task SendEmailAsync(string recipientEmail, string subject, string body)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress("Notification", _senderEmail));  
        message.To.Add(new MailboxAddress("Recipient", recipientEmail));  
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            TextBody = body,  
            HtmlBody = $"<p>{body}</p>"  
        };
        message.Body = bodyBuilder.ToMessageBody();

        
        using (var client = new SmtpClient())
        {
            try
            {
                await client.ConnectAsync(_smtpHost, _smtpPort, false);
                await client.AuthenticateAsync(_senderEmail, _senderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotificationApi.Models;
using NotificationApi.Services;
using System;
using System.Threading.Tasks;

namespace NotificationApi.Controllers
{
    [ApiController]
    [Route("api/message")]
    public class MessageController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IUserService _userService;  // Add IUserService
        private readonly ILogger<MessageController> _logger;

        public MessageController(IEmailService emailService, ISmsService smsService, IUserService userService, ILogger<MessageController> logger)
        {
            _emailService = emailService;
            _smsService = smsService;
            _userService = userService;  
            _logger = logger;
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            _logger.LogInformation($"[{DateTime.UtcNow}] Received request to send email. Email: {request.Email}, Subject: {request.Subject}");

            try
            {
                _logger.LogInformation($"[{DateTime.UtcNow}] Initiating email sending process.");
                await _emailService.SendEmailAsync(request.Email, request.Subject, request.Body);

                _logger.LogInformation($"[{DateTime.UtcNow}] Email sent successfully to {request.Email}.");
                return Ok(new { Message = "Email sent successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{DateTime.UtcNow}] Error occurred while sending email to {request.Email}. Exception: {ex.Message}");
                return StatusCode(500, new { Message = "Failed to send email. Please try again later." });
            }
        }

        [HttpPost("send-sms")]
        public async Task<IActionResult> SendSms([FromBody] SmsRequest request)
        {
            _logger.LogInformation($"[{DateTime.UtcNow}] Received request to send SMS. PhoneNumber: {request.PhoneNumber}");

            try
            {
                _logger.LogInformation($"[{DateTime.UtcNow}] Initiating SMS sending process.");
                await _smsService.SendSmsAsync(request.PhoneNumber, request.Message);

                _logger.LogInformation($"[{DateTime.UtcNow}] SMS sent successfully to {request.PhoneNumber}.");
                return Ok(new { Message = "SMS sent successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{DateTime.UtcNow}] Error occurred while sending SMS to {request.PhoneNumber}. Exception: {ex.Message}");
                return StatusCode(500, new { Message = "Failed to send SMS. Please try again later." });
            }
        }

        [HttpPost("notify-user")]
        public async Task<IActionResult> NotifyUser([FromBody] NotificationRequest request)
        {
            _logger.LogInformation($"[{DateTime.UtcNow}] Received request to notify user. UserId: {request.UserId}, NotificationType: {request.NotificationType}");

            if (request == null)
            {
                _logger.LogWarning($"[{DateTime.UtcNow}] Invalid request received for user notification.");
                return BadRequest("Invalid request");
            }

            var user = await _userService.GetUserByIdAsync(request.UserId);

            if (user == null)
            {
                _logger.LogWarning($"[{DateTime.UtcNow}] User with ID {request.UserId} not found.");
                return NotFound("User not found");
            }

            try
            {
                await _userService.SendNotificationAsync(user, request.Message, request.NotificationType);

                _logger.LogInformation($"[{DateTime.UtcNow}] Notification sent successfully to user {user.Id}.");
                return Ok("Notification sent");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{DateTime.UtcNow}] Error occurred while sending notification to user {user.Id}. Exception: {ex.Message}");
                return StatusCode(500, new { Message = "Failed to send notification. Please try again later." });
            }
        }
    }
}

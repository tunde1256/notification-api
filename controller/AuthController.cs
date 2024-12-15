using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NotificationApi.Data;
using NotificationApi.Models;
using NotificationApi.Services;
using System;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.Extensions.Configuration; 
using Microsoft.Extensions.Logging;       

namespace NotificationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MongoDbContext _mongoDbContext;
        private readonly IEmailService _emailService;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger; 

        public AuthController(MongoDbContext mongoDbContext, IEmailService emailService, JwtService jwtService, ILogger<AuthController> logger)
        {
            _mongoDbContext = mongoDbContext;
            _emailService = emailService;
            _jwtService = jwtService;
            _logger = logger;  
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
                return BadRequest("Email and Password are required.");

            if (await UserExists(user.Email))
                return Conflict("A user with this email already exists.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            await _mongoDbContext.Users.InsertOneAsync(user);

            _logger.LogInformation("New user registered: {Email}", user.Email);

            var emailSubject = "Welcome to Notification API!";
            var emailBody = $"Hi {user.Name},\n\nThank you for registering with us. We are excited to have you on board!\n\nBest Regards,\nNotification API Team";
            SendEmailInBackground(user.Email, emailSubject, emailBody);

            return CreatedAtAction(nameof(Login), new { id = user.Id }, user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Password))
                return BadRequest("Email and Password are required.");

            var user = await _mongoDbContext.Users
                .Find(u => u.Email.ToLower() == loginRequest.Email.ToLower())
                .FirstOrDefaultAsync();

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
            {
                _logger.LogWarning("Failed login attempt for email: {Email}", loginRequest.Email);  // Log failed login attempt
                return Unauthorized("Invalid credentials.");
            }

            _logger.LogInformation("User logged in successfully: {Email}", user.Email);

            var token = _jwtService.GenerateJwtToken(user);

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.PhoneNumber,
                user.NotificationPreference,
                Token = token 
            });
        }

        [HttpDelete("delete/{email}")]
        public async Task<IActionResult> Delete(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");

            var user = await _mongoDbContext.Users
                .Find(u => u.Email.ToLower() == email.ToLower())
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found.");

            _logger.LogInformation("Deleting user: {Email}", email);

            await _mongoDbContext.Users.DeleteOneAsync(u => u.Email.ToLower() == email.ToLower());

            var emailSubject = "Account Deleted";
            var emailBody = $"Hi {user.Name},\n\nYour account associated with {email} has been successfully deleted.\n\nBest Regards,\nNotification API Team";
            SendEmailInBackground(user.Email, emailSubject, emailBody);

            return NoContent();
        }

        private async Task<bool> UserExists(string email)
        {
            var existingUser = await _mongoDbContext.Users
                .Find(u => u.Email.ToLower() == email.ToLower())
                .FirstOrDefaultAsync();
            return existingUser != null;
        }

        private void SendEmailInBackground(string email, string subject, string body)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendEmailAsync(email, subject, body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to {Email}", email); 
                }
            });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NotificationApi.Data;
using NotificationApi.Models;
using NotificationApi.Services;
using System;
using System.Threading.Tasks;
using BCrypt.Net;

namespace NotificationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MongoDbContext _mongoDbContext;
        private readonly IEmailService _emailService;

        public AuthController(MongoDbContext mongoDbContext, IEmailService emailService)
        {
            _mongoDbContext = mongoDbContext;
            _emailService = emailService;
        }

        // POST api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
                return BadRequest("Email and Password are required.");

            // Check if the user already exists
            if (await UserExists(user.Email))
                return Conflict("A user with this email already exists.");

            // Hash the password before saving
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            // Insert the new user
            await _mongoDbContext.Users.InsertOneAsync(user);

            // Send welcome email asynchronously
            var emailSubject = "Welcome to Notification API!";
            var emailBody = $"Hi {user.Name},\n\nThank you for registering with us. We are excited to have you on board!\n\nBest Regards,\nNotification API Team";

            SendEmailInBackground(user.Email, emailSubject, emailBody);

            return CreatedAtAction(nameof(Login), new { id = user.Id }, user);
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Password))
                return BadRequest("Email and Password are required.");

            // Find the user by email
            var user = await _mongoDbContext.Users
                .Find(u => u.Email.ToLower() == loginRequest.Email.ToLower())
                .FirstOrDefaultAsync();

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
                return Unauthorized("Invalid credentials.");

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.PhoneNumber,
                user.NotificationPreference
            });
        }

        // DELETE api/auth/delete/{email}
        [HttpDelete("delete/{email}")]
        public async Task<IActionResult> Delete(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");

            // Find the user by email
            var user = await _mongoDbContext.Users
                .Find(u => u.Email.ToLower() == email.ToLower())
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found.");

            // Delete the user
            await _mongoDbContext.Users.DeleteOneAsync(u => u.Email.ToLower() == email.ToLower());

            // Send account deletion email asynchronously
            var emailSubject = "Account Deleted";
            var emailBody = $"Hi {user.Name},\n\nYour account associated with {email} has been successfully deleted.\n\nBest Regards,\nNotification API Team";

            SendEmailInBackground(user.Email, emailSubject, emailBody);

            return NoContent();
        }

        // Helper method: Check if a user exists
        private async Task<bool> UserExists(string email)
        {
            var existingUser = await _mongoDbContext.Users
                .Find(u => u.Email.ToLower() == email.ToLower())
                .FirstOrDefaultAsync();
            return existingUser != null;
        }

        // Helper method: Send email in the background
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
                    // Log the exception or handle it accordingly
                    Console.WriteLine($"Failed to send email to {email}: {ex.Message}");
                }
            });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using NotificationApi.Models;
using NotificationApi.Services;
using NotificationApi.Data;  
using MimeKit; 
using System.Linq;  
using Microsoft.EntityFrameworkCore;  

namespace NotificationApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public AuthController(IAuthService authService, AppDbContext context, EmailService emailService)
        {
            _authService = authService;
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { Message = "User with this email already exists." });
                }

                var registeredUser = await _authService.RegisterAsync(user, user.Password);

                string subject = "Registration Successful!";
                string body = "You are welcome, and your registration was successful.";
                await _emailService.SendEmailAsync(user.Email, subject, body);

                return Ok(new { Message = "Registration successful.", User = registeredUser });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var token = await _authService.LoginAsync(loginRequest.Email, loginRequest.Password);
                return Ok(new { Message = "Login successful.", Token = token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
        }

        [HttpDelete("deleteUser/{email}")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            try
            {
                // Find the user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found." });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                string subject = "Account Deleted";
                string body = "Your account has been successfully deleted.";
                await _emailService.SendEmailAsync(user.Email, subject, body);

                return NoContent(); 
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}

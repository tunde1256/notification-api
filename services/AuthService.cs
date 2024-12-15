using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NotificationApi.Models;
using NotificationApi.Services;
using NotificationApi.Data;
using System.Threading.Tasks;

namespace NotificationApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly MongoDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(MongoDbContext context, JwtService jwtService, ILogger<AuthService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<User> RegisterAsync(User user, string password)
        {
            _logger.LogInformation("Attempting to register user with email: {Email}", user.Email);

            var existingUser = await _context.Users.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed: User with email {Email} already exists.", user.Email);
                throw new Exception("User with this email already exists.");
            }

            user.Password = HashPassword(password); 
            await _context.Users.InsertOneAsync(user); 

            _logger.LogInformation("User with email {Email} successfully registered.", user.Email);
            return user;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            _logger.LogInformation("Attempting to log in user with email: {Email}", email);

            var user = await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null || !VerifyPassword(password, user.Password))
            {
                _logger.LogWarning("Login failed: Invalid email or password for email {Email}.", email);
                throw new Exception("Invalid email or password.");
            }

            _logger.LogInformation("User with email {Email} successfully logged in.", email);
            return _jwtService.GenerateJwtToken(user);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }
    }
}

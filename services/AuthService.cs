using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotificationApi.Models;
using NotificationApi.Services;
using NotificationApi.Data;  

namespace NotificationApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly MongoDbContext _context;
        private readonly JwtService _jwtService;

        public AuthService(MongoDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<User> RegisterAsync(User user, string password)
        {
            var existingUser = await _context.Users.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
            if (existingUser != null)
                throw new Exception("User with this email already exists.");

            user.Password = HashPassword(password); // Hash the password
            await _context.Users.InsertOneAsync(user); // Insert user into MongoDB

            return user;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();

            if (user == null || !VerifyPassword(password, user.Password))
                throw new Exception("Invalid email or password.");

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

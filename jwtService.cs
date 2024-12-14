using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NotificationApi.Models;

namespace NotificationApi.Services
{
    public class JwtService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly double _expiryHours;
        private readonly ILogger<JwtService> _logger;

        public JwtService(string secretKey, string issuer, string audience, double expiryHours, ILogger<JwtService> logger)
        {
            _secretKey = secretKey;
            _issuer = issuer;
            _audience = audience;
            _expiryHours = expiryHours;
            _logger = logger;

            _logger.LogInformation("JwtService initialized with Issuer: {Issuer}, Audience: {Audience}, ExpiryHours: {ExpiryHours}", 
                                    _issuer, _audience, _expiryHours);
        }

        public string GenerateJwtToken(User user)
        {
            try
            {
                _logger.LogInformation("Generating JWT token for user: {UserId}, Email: {Email}", user.Id, user.Email);

                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
                var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

                _logger.LogDebug("Signing credentials created.");

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),  // Subject (User ID)
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),        // Email claim
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token ID
                    new Claim("name", user.Name)                                 // Custom claim
                };

                _logger.LogDebug("Claims created for user: {UserId}", user.Id);

                var token = new JwtSecurityToken(
                    issuer: _issuer,
                    audience: _audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(_expiryHours),
                    signingCredentials: signingCredentials
                );

                var jwt = new JwtSecurityTokenHandler().WriteToken(token);
                _logger.LogInformation("JWT token successfully generated for user: {UserId}", user.Id);

                return jwt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating JWT token for user: {UserId}", user.Id);
                throw;
            }
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ShiftOne.Application.Interfaces.Common;
using ShiftOne.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShiftOne.Application.Services.Common {
    public class TokenService : ITokenService {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config) {
            _config = config;
        }

        // Generates a signed JWT access token containing the user's claims
        public string GenerateAccessToken(User user) {
            var keyStr = _config["Jwt:Key"];

            if(string.IsNullOrEmpty(keyStr))
                throw new InvalidOperationException("JWT Key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"], claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:AccessTokenExpirationMinutes"] ?? "60")),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Generates a unique refresh token using a base64-encoded GUID
        public string GenerateRefreshToken() {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }
}
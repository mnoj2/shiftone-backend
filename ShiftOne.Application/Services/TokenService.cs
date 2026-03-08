using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ShiftOne.Application.Interfaces;
using ShiftOne.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShiftOne.Application.Services {
    public class TokenService : ITokenService {

        private readonly IConfiguration _config;

        public TokenService(IConfiguration config) {
            _config = config;
        }

        public string GenerateAccessToken(User user) {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("id", user.Id.ToString()),
                new Claim("name", user.Name),
                new Claim("role", user.Role)
            };
            var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"], claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:AccessTokenExpirationMinutes"])),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken() {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }
}

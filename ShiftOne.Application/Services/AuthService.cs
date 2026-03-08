using Microsoft.Extensions.Configuration;
using ShiftOne.Application.Dtos;
using ShiftOne.Application.Interfaces;
using ShiftOne.Domain.Interfaces;

namespace ShiftOne.Application.Services {
    public class AuthService : IAuthService {

        private readonly IUserRepository _repo;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository repo, ITokenService tokenService, IConfiguration config) {
            _repo = repo;
            _tokenService = tokenService;
            _config = config;
        }

        public async Task<TokenResponseDto?> LoginAsync(LoginDto dto) {
            var user = await _repo.GetByEmailAsync(dto.Email);
            if(user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null; 

            var response = new TokenResponseDto {
                AccessToken = _tokenService.GenerateAccessToken(user),
                RefreshToken = _tokenService.GenerateRefreshToken(),
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:AccessTokenExpirationMinutes"]))
            };

            user.RefreshToken = response.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            bool isUpdated = await _repo.UpdateRefreshTokenAsync(user.Id, user.RefreshToken, user.RefreshTokenExpiryTime.Value);
            if(!isUpdated)
                return null; 

            return response;
        }
    }
}

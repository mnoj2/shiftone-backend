using ShiftOne.Application.Dtos;
using ShiftOne.Application.Interfaces.Common;
using ShiftOne.Domain.Interfaces.Common;

namespace ShiftOne.Application.Services.Common {
    public class AuthService : IAuthService {
        private readonly IUserRepository _repo;
        private readonly ITokenService _tokenService;

        public AuthService(IUserRepository repo, ITokenService tokenService) {
            _repo = repo;
            _tokenService = tokenService;
        }

        // Authenticates the user by email and password, and returns a token pair if successful
        public async Task<TokenResponseDto?> LoginAsync(LoginDto dto) {
            var user = await _repo.GetByEmailAsync(dto.Email);

            if(user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            var response = new TokenResponseDto {
                AccessToken = _tokenService.GenerateAccessToken(user),
                RefreshToken = _tokenService.GenerateRefreshToken()
            };

            user.RefreshToken = response.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            bool isUpdated = await _repo.UpdateRefreshTokenAsync(user.Id, user.RefreshToken, user.RefreshTokenExpiryTime.Value);

            if(!isUpdated)
                return null;

            return response;
        }

        // Validates the provided refresh token and issues a new token pair if still valid
        public async Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken) {
            var user = await _repo.GetByRefreshTokenAsync(refreshToken);

            if(user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return null;

            var response = new TokenResponseDto {
                AccessToken = _tokenService.GenerateAccessToken(user),
                RefreshToken = _tokenService.GenerateRefreshToken()
            };

            user.RefreshToken = response.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            bool isUpdated = await _repo.UpdateRefreshTokenAsync(user.Id, user.RefreshToken, user.RefreshTokenExpiryTime.Value);

            if(!isUpdated)
                return null;

            return response;
        }

        // Revokes the refresh token by clearing it and setting its expiry to the past
        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken) {
            var user = await _repo.GetByRefreshTokenAsync(refreshToken);

            if(user == null)
                return false;

            return await _repo.UpdateRefreshTokenAsync(user.Id, string.Empty, DateTime.UtcNow.AddDays(-1));
        }
    }
}
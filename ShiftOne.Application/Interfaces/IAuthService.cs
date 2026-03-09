using ShiftOne.Application.Dtos;

namespace ShiftOne.Application.Interfaces {
    public interface IAuthService {
        Task<TokenResponseDto?> LoginAsync(LoginDto dto);
        Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    }
}

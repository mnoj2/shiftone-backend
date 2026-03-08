using ShiftOne.Application.Dtos;

namespace ShiftOne.Application.Interfaces {
    public interface IAuthService {
        Task<TokenResponseDto?> LoginAsync(LoginDto dto);
    }
}

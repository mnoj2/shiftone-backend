using ShiftOne.Domain.Entities;

namespace ShiftOne.Application.Interfaces {
    public interface ITokenService {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}

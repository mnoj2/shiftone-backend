using ShiftOne.Domain.Entities;

namespace ShiftOne.Application.Interfaces.Common {
    public interface ITokenService {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}

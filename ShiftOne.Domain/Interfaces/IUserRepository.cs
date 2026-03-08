using ShiftOne.Domain.Entities;

namespace ShiftOne.Domain.Interfaces {
    public interface IUserRepository {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiryTime);
    }
}

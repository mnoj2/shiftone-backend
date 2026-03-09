using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftOne.Domain.Entities;

namespace ShiftOne.Domain.Interfaces.Common {
    public interface IUserRepository {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task<bool> AddAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiryTime);
        Task<List<User>> GetAllWorkersAsync();
        Task<List<User>> GetAllUsersAsync();
    }
}

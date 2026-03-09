using ShiftOne.Application.Interfaces.Admin;
using ShiftOne.Domain.Entities;
using ShiftOne.Domain.Interfaces.Common;

namespace ShiftOne.Application.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepo;

        public AdminService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<List<UserDto>?> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllUsersAsync();
            return users?.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role
            }).ToList();
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var u = await _userRepo.GetByIdAsync(id);
            if (u == null) return null;
            return new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role
            };
        }

        public async Task<bool> CreateUserAsync(CreateUserDto dto)
        {
            var existing = await _userRepo.GetByEmailAsync(dto.Email);
            if (existing != null) return false;

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone,
                Role = dto.Role
            };

            return await _userRepo.AddAsync(user);
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return false;

            if (!string.IsNullOrWhiteSpace(dto.Name)) user.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email;
            if (!string.IsNullOrWhiteSpace(dto.Phone)) user.Phone = dto.Phone;
            if (!string.IsNullOrWhiteSpace(dto.Role)) user.Role = dto.Role;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            return await _userRepo.UpdateAsync(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepo.DeleteAsync(id);
        }
    }
}

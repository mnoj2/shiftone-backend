using ShiftOne.Application.Dtos;
using ShiftOne.Application.Interfaces;
using ShiftOne.Domain.Entities;
using ShiftOne.Domain.Interfaces.Common;

namespace ShiftOne.Application.Services {
    public class AdminService : IAdminService {
        private readonly IUserRepository _userRepo;

        public AdminService(IUserRepository userRepo) {
            _userRepo = userRepo;
        }

        // Retrieves all users and maps them to UserDto list
        public async Task<List<UserDto>?> GetAllUsersAsync() {
            var users = await _userRepo.GetAllUsersAsync();

            return users?.Select(u => new UserDto {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role
            }).ToList();
        }

        // Retrieves a single user by ID and maps them to UserDto
        public async Task<UserDto?> GetUserByIdAsync(int id) {
            var u = await _userRepo.GetByIdAsync(id);

            if(u == null)
                return null;

            return new UserDto {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role
            };
        }

        // Creates a new user after checking for duplicate email and hashing the password
        public async Task<bool> CreateUserAsync(CreateUserDto dto) {
            var existing = await _userRepo.GetByEmailAsync(dto.Email);

            if(existing != null)
                return false;

            var user = new User {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone,
                Role = dto.Role
            };

            return await _userRepo.AddAsync(user);
        }

        // Updates an existing user's details, only overwriting fields that are provided
        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto dto) {
            var user = await _userRepo.GetByIdAsync(id);

            if(user == null)
                return false;

            if(!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name;
            if(!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;
            if(!string.IsNullOrWhiteSpace(dto.Phone))
                user.Phone = dto.Phone;
            if(!string.IsNullOrWhiteSpace(dto.Role))
                user.Role = dto.Role;

            if(!string.IsNullOrWhiteSpace(dto.Password)) {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            return await _userRepo.UpdateAsync(user);
        }

        // Deletes a user by their ID
        public async Task<bool> DeleteUserAsync(int id) {
            return await _userRepo.DeleteAsync(id);
        }
    }
}
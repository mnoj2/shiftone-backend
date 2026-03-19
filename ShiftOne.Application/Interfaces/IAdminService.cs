using ShiftOne.Application.Dtos;

namespace ShiftOne.Application.Interfaces
{
    public interface IAdminService
    {
        Task<List<UserDto>?> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<bool> CreateUserAsync(CreateUserDto dto);
        Task<bool> UpdateUserAsync(int id, UpdateUserDto dto);
        Task<bool> DeleteUserAsync(int id);
        Task<FormExtractDto?> ExtractFormDataAsync(Stream fileStream, string fileName, string contentType);
    }

}

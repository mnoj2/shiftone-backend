using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Application.Dtos {
    public class CreateUserDto {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = ShiftOne.Domain.Constants.UserRoles.Worker;
    }
    public class UpdateUserDto {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        [Required]
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
    public class FormExtractDto {
        [Required]
        public string Name { get; set; } = "";
        [Required]
        public string Email { get; set; } = "";
        [Required]
        public string Phone { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
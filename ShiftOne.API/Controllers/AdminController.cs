using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftOne.Application.Interfaces;
using ShiftOne.Application.Dtos;

using ShiftOne.Domain.Constants;

namespace ShiftOne.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class AdminController : ControllerBase {

        private readonly IAdminService _adminService;
        private readonly IOcrService _ocrService;

        public AdminController(IAdminService adminService, IOcrService ocrService) {
            _adminService = adminService;
            _ocrService = ocrService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers() {
            var result = await _adminService.GetAllUsersAsync();
            if(result == null) {
                return StatusCode(500, "Load failed");
            }
            return Ok(result);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(int id) {
            var result = await _adminService.GetUserByIdAsync(id);
            if(result == null) {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto) {
            var success = await _adminService.CreateUserAsync(dto);
            if(!success) {
                return BadRequest(new { message = "User with this email already exists" });
            }
            return Ok(new { message = "User created successfully" });
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto) {
            var success = await _adminService.UpdateUserAsync(id, dto);
            if(!success) {
                return BadRequest(new { message = "Update failed" });
            }
            return Ok(new { message = "User updated successfully" });
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id) {
            var success = await _adminService.DeleteUserAsync(id);
            if(!success) {
                return BadRequest(new { message = "Delete failed" });
            }
            return Ok(new { message = "User deleted successfully" });
        }

        [HttpPost("scan-form")]
        public async Task<IActionResult> ScanForm(IFormFile file) {
            if(file == null || file.Length == 0) {
                return BadRequest(new { message = "No file provided" });
            }

            var result = await _ocrService.ExtractFormDataAsync(
                file.OpenReadStream(),
                file.FileName,
                file.ContentType
            );

            if(result == null) {
                return StatusCode(500, new { message = "OCR extraction failed" });
            }
            return Ok(result);
        }

    }
}

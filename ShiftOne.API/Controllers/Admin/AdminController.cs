using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ShiftOne.Application.Interfaces;
using ShiftOne.Application.Interfaces.Admin;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShiftOne.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase {
        private readonly IAttendanceService _attendanceService;
        private readonly IAdminService _adminService;

        public AdminController(IAttendanceService attendanceService, IAdminService adminService) {
            _attendanceService = attendanceService;
            _adminService = adminService;
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

        [HttpDelete("attendance/{userId}")]
        public async Task<IActionResult> DeleteAttendance(int userId) {
            var success = await _attendanceService.DeleteUserAttendanceAsync(userId);
            if(!success) {
                return StatusCode(500, "Delete failed");
            }
            return Ok(new { message = "Attendance records deleted successfully" });
        }

    }
}

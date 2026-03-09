using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftOne.Application.Interfaces;
using ShiftOne.Application.Interfaces.Admin;

namespace ShiftOne.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IAdminService _adminService;

        public AdminController(IAttendanceService attendanceService, IAdminService adminService)
        {
            _attendanceService = attendanceService;
            _adminService = adminService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _adminService.GetAllUsersAsync();
            return result == null ? StatusCode(500, "Load failed") : Ok(result);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var result = await _adminService.GetUserByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var success = await _adminService.CreateUserAsync(dto);
            return success ? Ok(new { message = "User created successfully" }) : BadRequest(new { message = "Create failed (Email may already exist)" });
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var success = await _adminService.UpdateUserAsync(id, dto);
            return success ? Ok(new { message = "User updated successfully" }) : BadRequest(new { message = "Update failed" });
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _adminService.DeleteUserAsync(id);
            return success ? Ok(new { message = "User deleted successfully" }) : BadRequest(new { message = "Delete failed" });
        }

        [HttpDelete("attendance/{userId}")]
        public async Task<IActionResult> DeleteAttendance(int userId)
        {
            var success = await _attendanceService.DeleteUserAttendanceAsync(userId);
            return success ? Ok(new { message = "Attendance records deleted successfully" }) : StatusCode(500, "Delete failed");
        }

        [HttpGet("status")]
        public IActionResult GetStatus() => Ok(new { message = "Admin API is active." });
    }
}

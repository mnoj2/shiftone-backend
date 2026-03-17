using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftOne.Application.Interfaces;
using System.Security.Claims;

namespace ShiftOne.API.Controllers {
    [ApiController]
    [Route("api/worker")]
    [Authorize(Roles = "Worker")]
    public class WorkerController : ControllerBase {

        private readonly IAttendanceService _attendanceService;

        public WorkerController(IAttendanceService attendanceService) {
            _attendanceService = attendanceService;
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn() {
            var userId = GetUserId();
            var message = await _attendanceService.SignInAsync(userId);
            if(message == null) {
                return BadRequest(new { message = "Sign in failed" });
            }
            return Ok(new { message });
        }

        [HttpPost("signoff")]
        public async Task<IActionResult> SignOff() {
            var userId = GetUserId();
            var message = await _attendanceService.SignOffAsync(userId);
            if(message == null) {
                return BadRequest(new { message = "Sign off failed" });
            }
            return Ok(new { message });
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetToday() {
            var result = await _attendanceService.GetTodayInfoAsync(GetUserId());
            if(result == null) {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory() {
            var userId = GetUserId();
            var result = await _attendanceService.GetWorkerHistoryAsync(userId);
            if(result == null) {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost("manual-signoff")]
        public async Task<IActionResult> ManualSignOff([FromBody] ManualSignOffDto req) {
            var userId = GetUserId();
            var success = await _attendanceService.ManualSignOffAsync(userId, req.Date, req.SignOffTime);
            if(!success) {
                return BadRequest(new { message = "Manual sign off failed" });
            }
            return Ok(new { message = "Manual sign off successful" });
        }

        private int GetUserId() {
            var idClaim = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int id) ? id : 0;
        }
    }

    public class ManualSignOffDto {
        public DateTime Date { get; set; }
        public DateTime SignOffTime { get; set; }
    }
}
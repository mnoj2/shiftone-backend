using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftOne.Application.Interfaces;
using ShiftOne.Application.Interfaces.Worker;
using System.Security.Claims;

namespace ShiftOne.API.Controllers.Worker
{
    [ApiController]
    [Route("api/worker")]
    [Authorize(Roles = "Worker")]
    public class WorkerController : ControllerBase
    {
        private readonly IWorkerService _workerService;
        private readonly IAttendanceService _attendanceService;

        public WorkerController(IWorkerService workerService, IAttendanceService attendanceService)
        {
            _workerService = workerService;
            _attendanceService = attendanceService;
        }


        [HttpPost("signin")]
        public async Task<IActionResult> SignIn() {
            var userId = GetUserId();
            var message = await _attendanceService.SignInAsync(userId);
            return message == null ? BadRequest(new { message = "Sign in failed" }) : Ok(new { message });
        }

        [HttpPost("signoff")]
        public async Task<IActionResult> SignOff() {
            var userId = GetUserId();
            var message = await _attendanceService.SignOffAsync(userId);
            return message == null ? BadRequest(new { message = "Sign off failed" }) : Ok(new { message });
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetToday() {
            var result = await _attendanceService.GetTodayInfoAsync(GetUserId());
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory() {
            var userId = GetUserId();
            var result = await _attendanceService.GetWorkerHistoryAsync(userId);
            return result == null ? NotFound() : Ok(result);
        }


        [HttpPost("manual-signoff")]
        public async Task<IActionResult> ManualSignOff([FromBody] ManualSignOffDto req) {
            var userId = GetUserId();
            var success = await _attendanceService.ManualSignOffAsync(userId, req.Date, req.SignOffTime);
            return success ? Ok(new { message = "Manual sign off successful" }) : BadRequest(new { message = "Manual sign off failed" });
        }

        private int GetUserId() {
            var idClaim = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int id) ? id : 0;
        }
    }

    public class ConfirmAutoDto { public DateTime Date { get; set; } public DateTime ActualTime { get; set; } }
    public class ManualSignOffDto { public DateTime Date { get; set; } public DateTime SignOffTime { get; set; } }
}

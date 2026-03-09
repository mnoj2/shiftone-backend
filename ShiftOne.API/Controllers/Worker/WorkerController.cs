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

        [HttpGet("home")]
        public async Task<IActionResult> GetHome()
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var result = await _workerService.GetHomeAsync(userId);
            return result == null ? StatusCode(500, "Load failed") : Ok(result);
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] LocationDto loc)
        {
            var userId = GetUserId();
            var message = await _attendanceService.SignInAsync(userId, loc.Lat, loc.Lng);
            return message == null ? BadRequest(new { message = "Sign in failed (Location/Duplicate)" }) : Ok(new { message });
        }

        [HttpPost("signoff")]
        public async Task<IActionResult> SignOff([FromBody] LocationDto loc)
        {
            var userId = GetUserId();
            var message = await _attendanceService.SignOffAsync(userId, loc.Lat, loc.Lng);
            return message == null ? BadRequest(new { message = "Sign off failed (Location/Not signed in)" }) : Ok(new { message });
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetToday()
        {
            var userId = GetUserId();
            var result = await _attendanceService.GetTodayInfoAsync(userId);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = GetUserId();
            var result = await _attendanceService.GetWorkerHistoryAsync(userId);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("confirm-auto")]
        public async Task<IActionResult> ConfirmAuto([FromBody] ConfirmAutoDto req)
        {
            var userId = GetUserId();
            var success = await _attendanceService.ConfirmAutoSignOffAsync(userId, req.Date, req.ActualTime);
            return success ? Ok(new { message = "Confirmed successfully" }) : BadRequest(new { message = "Confirmation failed" });
        }

        private int GetUserId()
        {
            var idClaim = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int id) ? id : 0;
        }
    }

    public class LocationDto { public double Lat { get; set; } public double Lng { get; set; } }
    public class ConfirmAutoDto { public DateTime Date { get; set; } public DateTime ActualTime { get; set; } }
}

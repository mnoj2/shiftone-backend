using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftOne.Application.Interfaces;
using ShiftOne.Application.Dtos.Worker;

namespace ShiftOne.API.Controllers.Supervisor
{
    [ApiController]
    [Route("api/supervisor")]
    [Authorize(Roles = "Supervisor")]
    public class SupervisorController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public SupervisorController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpGet("home-summary")]
        public async Task<IActionResult> GetHomeSummary([FromQuery] DateTime? date) {
            var result = await _attendanceService.GetSupervisorHomeSummaryAsync(date);
            return result == null ? StatusCode(500, "Load failed") : Ok(result);
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics([FromQuery] int month, [FromQuery] int year)
        {
            var result = await _attendanceService.GetSupervisorAnalyticsAsync(month, year);
            return result == null ? StatusCode(500, "Load failed") : Ok(result);
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetRange([FromQuery] DateTime start, [FromQuery] DateTime end) {
            var result = await _attendanceService.GetByDateRangeAsync(start, end);
            return result == null ? NotFound() : Ok(result);
        }
    }
}

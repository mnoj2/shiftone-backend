using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftOne.Application.Interfaces;

namespace ShiftOne.API.Controllers.Supervisor {
    [ApiController]
    [Route("api/supervisor")]
    [Authorize(Roles = "Supervisor")]
    public class SupervisorController : ControllerBase {
        private readonly IAttendanceService _attendanceService;

        public SupervisorController(IAttendanceService attendanceService) {
            _attendanceService = attendanceService;
        }

        [HttpGet("home-summary")]
        public async Task<IActionResult> GetHomeSummary([FromQuery] DateTime? date) {
            var result = await _attendanceService.GetSupervisorHomeSummaryAsync(date);
            if(result == null) {
                return StatusCode(500, "Load failed");
            }
            return Ok(result);
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics([FromQuery] int month, [FromQuery] int year) {
            var result = await _attendanceService.GetSupervisorAnalyticsAsync(month, year);
            if(result == null) {
                return StatusCode(500, "Load failed");
            }
            return Ok(result);
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetRange([FromQuery] DateTime start, [FromQuery] DateTime end) {
            var result = await _attendanceService.GetByDateRangeAsync(start, end);
            if(result == null) {
                return NotFound();
            }
            return Ok(result);
        }
    }
}
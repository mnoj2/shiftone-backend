using System;
using System.Threading.Tasks;
using ShiftOne.Application.Dtos.Worker;
using ShiftOne.Application.Interfaces.Worker;
using ShiftOne.Domain.Interfaces;
using ShiftOne.Domain.Interfaces.Worker;

using Microsoft.Extensions.Configuration;

namespace ShiftOne.Application.Services.Worker
{
    public class WorkerService : IWorkerService
    {
        private readonly IShiftRepository _shiftRepo;
        private readonly IAttendanceRepository _attendanceRepo;
        private readonly IConfiguration _config;

        public WorkerService(IShiftRepository shiftRepo, IAttendanceRepository attendanceRepo, IConfiguration config)
        {
            _shiftRepo = shiftRepo;
            _attendanceRepo = attendanceRepo;
            _config = config;
        }

        private DateTime GetFactoryLocalDate()
        {
            var tzId = _config.GetValue<string>("FactoryLocation:TimeZone") ?? "India Standard Time";
            var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
        }

        public async Task<WorkerHomeDto?> GetHomeAsync(int workerId)
        {
            var today = GetFactoryLocalDate();
            
            var shift = await _shiftRepo.GetShiftByWorkerAndDateAsync(workerId, today);
            var attendance = await _attendanceRepo.GetTodayAttendanceAsync(workerId, today);
            var pendingAuto = await _attendanceRepo.GetPendingAutoSignOffAsync(workerId, today) != null;

            return new WorkerHomeDto
            {
                TodayShift = shift == null ? null : new ShiftDto
                {
                    Id = shift.Id,
                    Date = shift.Date,
                    Status = shift.Status,
                    StartTime = shift.SignInTime,
                    EndTime = shift.SignOffTime
                },
                Attendance = attendance == null ? null : new AttendanceSummaryDto
                {
                    SignedIn = attendance.SignedIn,
                    SignedOff = attendance.SignedOff,
                    TotalHours = attendance.TotalHours
                },
                HasPendingAutoSignOff = pendingAuto
            };
        }
    }
}

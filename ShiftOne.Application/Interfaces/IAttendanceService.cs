using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftOne.Application.Dtos.Worker;

namespace ShiftOne.Application.Interfaces
{
    public interface IAttendanceService
    {
        Task<string?> SignInAsync(int userId);
        Task<string?> SignOffAsync(int userId);
        Task<AttendanceInfoDto?> GetTodayInfoAsync(int userId);
        Task<List<WorkerDto>?> GetAllWorkersAsync();
        Task<List<AttendanceRecordDto>?> GetWorkerHistoryAsync(int userId);
        Task<List<AttendanceRecordDto>?> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<SupervisorAnalyticsDto?> GetSupervisorAnalyticsAsync(int month, int year);
        Task<SupervisorHomeDto?> GetSupervisorHomeSummaryAsync(DateTime? date = null);
        Task<bool> ConfirmAutoSignOffAsync(int userId, DateTime date, DateTime actualSignOffTime);
        Task<bool> ManualSignOffAsync(int userId, DateTime date, DateTime signOffTime);
        Task<bool> DeleteUserAttendanceAsync(int userId);
    }

    public class AttendanceInfoDto
    {
        public string Status { get; set; } = string.Empty;
        public DateTime? SignInTime { get; set; }
        public DateTime? SignOffTime { get; set; }
        public double? TotalHours { get; set; }
    }
}

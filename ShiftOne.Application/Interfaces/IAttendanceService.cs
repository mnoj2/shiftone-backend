using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftOne.Application.Dtos.Worker;

namespace ShiftOne.Application.Interfaces
{
    public interface IAttendanceService
    {
        Task<string?> SignInAsync(int userId, double lat, double lng);
        Task<string?> SignOffAsync(int userId, double lat, double lng);
        Task<AttendanceInfoDto?> GetTodayInfoAsync(int userId);
        Task<List<WorkerDto>?> GetAllWorkersAsync();
        Task<List<AttendanceRecordDto>?> GetWorkerHistoryAsync(int userId);
        Task<List<AttendanceRecordDto>?> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<List<MonthlySummaryDto>?> GetMonthlySummaryAsync(int month, int year);
        Task<SupervisorAnalyticsDto?> GetSupervisorAnalyticsAsync(int month, int year);
        Task<SupervisorDashboardStatsDto?> GetSupervisorStatsAsync();
        Task<SupervisorHomeDto?> GetSupervisorHomeSummaryAsync(DateTime? date = null);
        Task<bool> ConfirmAutoSignOffAsync(int userId, DateTime date, DateTime actualSignOffTime);
        Task<bool> DeleteUserAttendanceAsync(int userId);
        Task<int> GetAttendanceCountAsync(int userId);
    }

    public class AttendanceInfoDto
    {
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? SignInTime { get; set; }
        public DateTime? SignOffTime { get; set; }
        public double? TotalHours { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

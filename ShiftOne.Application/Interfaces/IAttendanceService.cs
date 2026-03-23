using ShiftOne.Application.Dtos;

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
        Task<SupervisorAnalyticsDto?> GetSupervisorAnalyticsAsync(int month, int year);
        Task<SupervisorHomeDto?> GetSupervisorHomeSummaryAsync(DateTime? date = null);
        Task<bool> ConfirmAutoSignOffAsync(int userId, DateTime date, DateTime actualSignOffTime);
        Task<bool> ManualSignOffAsync(int userId, DateTime date, DateTime signOffTime);
        Task<bool> DeleteUserAttendanceAsync(int userId);
    }
}

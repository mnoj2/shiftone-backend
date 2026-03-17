using ShiftOne.Domain.Entities;
using ShiftOne.Domain.Models;

namespace ShiftOne.Domain.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<bool> HasAttendanceAsync(int userId, DateTime date);
        Task<bool> AddAsync(Attendance attendance);
        Task<bool> UpdateAsync(Attendance attendance);
        Task<Attendance?> GetActiveShiftAsync(int userId, DateTime date);
        Task<Attendance?> GetTodayRecordAsync(int userId, DateTime date);
        Task<List<Attendance>> GetUserHistoryAsync(int userId);
        Task<int> GetUserHistoryCountAsync(int userId);
        Task<List<Attendance>> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<List<Attendance>> GetMonthlyRecordsAsync(int month, int year);
        Task<List<Attendance>> GetByDateAsync(DateTime date);
        Task<Attendance?> GetPendingAutoSignOffAsync(int userId, DateTime date);
        Task<AttendanceSummary?> GetTodayAttendanceAsync(int userId, DateTime date);
        Task<bool> DeleteByUserIdAsync(int userId);
        Task<Attendance?> GetShiftByWorkerAndDateAsync(int workerId, DateTime date);
    }
}

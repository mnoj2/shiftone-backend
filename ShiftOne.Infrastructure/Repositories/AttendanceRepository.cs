using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ShiftOne.Domain.Models;
using ShiftOne.Domain.Entities;
using ShiftOne.Domain.Interfaces;

namespace ShiftOne.Infrastructure.Repositories {
    public class AttendanceRepository : IAttendanceRepository {

        private readonly string _connectionString;
        public AttendanceRepository(IConfiguration configuration) {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found.");
        }


        private Attendance MapAttendance(SqlDataReader reader) {
            return new Attendance {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                SignInTime = reader.IsDBNull(reader.GetOrdinal("SignInTime")) ? null : reader.GetDateTime(reader.GetOrdinal("SignInTime")),
                SignOffTime = reader.IsDBNull(reader.GetOrdinal("SignOffTime")) ? null : reader.GetDateTime(reader.GetOrdinal("SignOffTime")),
                TotalHours = reader.IsDBNull(reader.GetOrdinal("TotalHours")) ? null : reader.GetDouble(reader.GetOrdinal("TotalHours"))
            };
        }

        public async Task<bool> HasAttendanceAsync(int userId, DateTime date) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_has_attendance", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Date", date.Date);
            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return result != null;
        }

        public async Task<bool> AddAsync(Attendance attendance) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_add_attendance", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", attendance.UserId);
            command.Parameters.AddWithValue("@Date", attendance.Date.Date);
            command.Parameters.AddWithValue("@Status", attendance.Status);
            command.Parameters.AddWithValue("@SignInTime", (object) attendance.SignInTime ?? DBNull.Value);
            command.Parameters.AddWithValue("@SignOffTime", (object) attendance.SignOffTime ?? DBNull.Value);
            command.Parameters.AddWithValue("@TotalHours", (object) attendance.TotalHours ?? DBNull.Value);
            var statusCodeParam = new SqlParameter("@status_code", SqlDbType.VarChar, 1) { Direction = ParameterDirection.Output };
            var statusMsgParam = new SqlParameter("@status_msg", SqlDbType.VarChar, -1) { Direction = ParameterDirection.Output };
            command.Parameters.Add(statusCodeParam);
            command.Parameters.Add(statusMsgParam);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            return statusCodeParam.Value?.ToString() == "s";
        }

        public async Task<bool> UpdateAsync(Attendance attendance) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_update_attendance", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", attendance.Id);
            command.Parameters.AddWithValue("@Status", attendance.Status);
            command.Parameters.AddWithValue("@SignInTime", (object) attendance.SignInTime ?? DBNull.Value);
            command.Parameters.AddWithValue("@SignOffTime", (object) attendance.SignOffTime ?? DBNull.Value);
            command.Parameters.AddWithValue("@TotalHours", (object) attendance.TotalHours ?? DBNull.Value);
            var statusCodeParam = new SqlParameter("@status_code", SqlDbType.VarChar, 1) { Direction = ParameterDirection.Output };
            var statusMsgParam = new SqlParameter("@status_msg", SqlDbType.VarChar, -1) { Direction = ParameterDirection.Output };
            command.Parameters.Add(statusCodeParam);
            command.Parameters.Add(statusMsgParam);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            return statusCodeParam.Value?.ToString() == "s";
        }

        public async Task<Attendance?> GetActiveShiftAsync(int userId, DateTime date) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_get_active_shift", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Date", date.Date);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if(await reader.ReadAsync())
                return MapAttendance(reader);
            return null;
        }

        public async Task<Attendance?> GetTodayRecordAsync(int userId, DateTime date) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_get_today_record", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Date", date.Date);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if(await reader.ReadAsync())
                return MapAttendance(reader);
            return null;
        }

        public async Task<List<Attendance>> GetUserHistoryAsync(int userId) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_get_user_history", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);
            await connection.OpenAsync();
            var list = new List<Attendance>();
            using var reader = await command.ExecuteReaderAsync();
            while(await reader.ReadAsync())
                list.Add(MapAttendance(reader));
            return list;
        }

        public async Task<int> GetUserHistoryCountAsync(int userId) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_get_user_history_count", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();
            return (int) await command.ExecuteScalarAsync();
        }

        public async Task<List<Attendance>> GetByDateRangeAsync(DateTime start, DateTime end) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_get_attendance_by_range", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Start", start.Date);
            command.Parameters.AddWithValue("@End", end.Date);
            await connection.OpenAsync();
            var list = new List<Attendance>();
            using var reader = await command.ExecuteReaderAsync();
            while(await reader.ReadAsync())
                list.Add(MapAttendance(reader));
            return list;
        }

        public async Task<List<Attendance>> GetMonthlyRecordsAsync(int month, int year) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_get_monthly_records", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Month", month);
            command.Parameters.AddWithValue("@Year", year);
            await connection.OpenAsync();
            var list = new List<Attendance>();
            using var reader = await command.ExecuteReaderAsync();
            while(await reader.ReadAsync())
                list.Add(MapAttendance(reader));
            return list;
        }

        public async Task<List<Attendance>> GetByDateAsync(DateTime date) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_get_attendance_by_date", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Date", date.Date);
            await connection.OpenAsync();
            var list = new List<Attendance>();
            using var reader = await command.ExecuteReaderAsync();
            while(await reader.ReadAsync())
                list.Add(MapAttendance(reader));
            return list;
        }

        public async Task<Attendance?> GetPendingAutoSignOffAsync(int userId, DateTime date) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_get_pending_auto_signoff", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Date", date.Date);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if(await reader.ReadAsync())
                return MapAttendance(reader);
            return null;
        }

        public async Task<AttendanceSummary?> GetTodayAttendanceAsync(int userId, DateTime date) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_get_today_attendance_summary", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Date", date.Date);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if(await reader.ReadAsync()) {
                return new AttendanceSummary {
                    SignedIn = !reader.IsDBNull(reader.GetOrdinal("SignInTime")),
                    SignedOff = !reader.IsDBNull(reader.GetOrdinal("SignOffTime")),
                    TotalHours = reader.IsDBNull(reader.GetOrdinal("TotalHours")) ? null : reader.GetDouble(reader.GetOrdinal("TotalHours"))
                };
            }
            return null;
        }

        public async Task<bool> DeleteByUserIdAsync(int userId) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_delete_attendance_by_userid", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);
            await connection.OpenAsync();
            var result = await command.ExecuteNonQueryAsync();
            return result >= 0;
        }

        public async Task<Attendance?> GetShiftByWorkerAndDateAsync(int workerId, DateTime date) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_get_shift_by_worker_date", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@WorkerId", workerId);
            command.Parameters.AddWithValue("@Date", date.Date);
            var statusCodeParam = new SqlParameter("@status_code", SqlDbType.VarChar, 1) { Direction = ParameterDirection.Output };
            var statusMsgParam = new SqlParameter("@status_msg", SqlDbType.VarChar, -1) { Direction = ParameterDirection.Output };
            command.Parameters.Add(statusCodeParam);
            command.Parameters.Add(statusMsgParam);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if(await reader.ReadAsync()) {
                return MapAttendance(reader);
            }
            return null;
        }

    }
}
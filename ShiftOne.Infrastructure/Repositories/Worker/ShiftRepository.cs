using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ShiftOne.Domain.Entities;
using ShiftOne.Domain.Interfaces.Worker;
using System.Data;

namespace ShiftOne.Infrastructure.Repositories.Worker
{
    public class ShiftRepository : IShiftRepository
    {
        private readonly string _connectionString;

        public ShiftRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        public async Task<Attendance?> GetShiftByWorkerAndDateAsync(int workerId, DateTime date)
        {
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

            if (await reader.ReadAsync())
            {
                return new Attendance
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    UserId = workerId,
                    Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    SignInTime = reader.IsDBNull(reader.GetOrdinal("SignInTime")) ? null : reader.GetDateTime(reader.GetOrdinal("SignInTime")),
                    SignOffTime = reader.IsDBNull(reader.GetOrdinal("SignOffTime")) ? null : reader.GetDateTime(reader.GetOrdinal("SignOffTime")),
                    TotalHours = reader.IsDBNull(reader.GetOrdinal("TotalHours")) ? null : reader.GetDouble(reader.GetOrdinal("TotalHours"))
                };
            }
            return null;
        }
    }
}

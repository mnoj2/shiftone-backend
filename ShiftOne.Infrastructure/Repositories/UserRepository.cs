using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ShiftOne.Domain.Entities;
using ShiftOne.Domain.Interfaces.Common;
using System.Data;

namespace ShiftOne.Infrastructure.Repositories {
    public class UserRepository : IUserRepository {

        private readonly string _connectionString;
        public UserRepository(IConfiguration configuration) {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found.");
        }


        private static User MapUser(SqlDataReader reader) {
            return new User {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                RefreshToken = reader.IsDBNull(reader.GetOrdinal("RefreshToken")) ? null : reader.GetString(reader.GetOrdinal("RefreshToken")),
                RefreshTokenExpiryTime = reader.IsDBNull(reader.GetOrdinal("RefreshTokenExpiryTime")) ? null : reader.GetDateTime(reader.GetOrdinal("RefreshTokenExpiryTime")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
            };
        }

        public async Task<User?> GetByEmailAsync(string email) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_get_user_by_email", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Email", email);
            var statusCodeParam = new SqlParameter("@status_code", SqlDbType.VarChar, 1) { Direction = ParameterDirection.Output };
            var statusMsgParam = new SqlParameter("@status_msg", SqlDbType.VarChar, -1) { Direction = ParameterDirection.Output };
            command.Parameters.Add(statusCodeParam);
            command.Parameters.Add(statusMsgParam);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if(await reader.ReadAsync()) return MapUser(reader);
            return null;
        }

        public async Task<User?> GetByIdAsync(int id) {
             using var connection = new SqlConnection(_connectionString);
             using var command = new SqlCommand("sp_get_user_by_id", connection);
             command.CommandType = CommandType.StoredProcedure;
             command.Parameters.AddWithValue("@Id", id);
             await connection.OpenAsync();
             using var reader = await command.ExecuteReaderAsync();
             if (await reader.ReadAsync()) return MapUser(reader);
             return null;
        }

        public async Task<bool> AddAsync(User user) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_add_user", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Name", user.Name);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@Phone", user.Phone);
            command.Parameters.AddWithValue("@Role", user.Role);
            var statusCodeParam = new SqlParameter("@status_code", SqlDbType.VarChar, 1) { Direction = ParameterDirection.Output };
            var statusMsgParam = new SqlParameter("@status_msg", SqlDbType.VarChar, -1) { Direction = ParameterDirection.Output };
            command.Parameters.Add(statusCodeParam);
            command.Parameters.Add(statusMsgParam);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            return (string)statusCodeParam.Value == "s";
        }

        public async Task<bool> UpdateAsync(User user) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_update_user", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", user.Id);
            command.Parameters.AddWithValue("@Name", user.Name);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@Phone", user.Phone);
            command.Parameters.AddWithValue("@Role", user.Role);
            await connection.OpenAsync();
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_delete_user", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", id);
            await connection.OpenAsync();
            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiryTime) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_add_refreshtoken_details", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", userId);
            command.Parameters.AddWithValue("@RefreshToken", refreshToken);
            command.Parameters.AddWithValue("@RefreshTokenExpiryTime", expiryTime);
            var statusCodeParam = new SqlParameter("@status_code", SqlDbType.VarChar, 1) { Direction = ParameterDirection.Output };
            var statusMsgParam = new SqlParameter("@status_msg", SqlDbType.VarChar, -1) { Direction = ParameterDirection.Output };
            command.Parameters.Add(statusCodeParam);
            command.Parameters.Add(statusMsgParam);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            return statusCodeParam.Value?.ToString() == "s";
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken) {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_get_user_by_refreshtoken", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@RefreshToken", refreshToken);
            var statusCodeParam = new SqlParameter("@status_code", SqlDbType.VarChar, 1) { Direction = ParameterDirection.Output };
            var statusMsgParam = new SqlParameter("@status_msg", SqlDbType.VarChar, -1) { Direction = ParameterDirection.Output };
            command.Parameters.Add(statusCodeParam);
            command.Parameters.Add(statusMsgParam);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if(await reader.ReadAsync()) return MapUser(reader);
            return null;
        }

        public async Task<List<User>> GetAllWorkersAsync() {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_get_all_workers", connection);
            command.CommandType = CommandType.StoredProcedure;
            await connection.OpenAsync();
            var list = new List<User>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) list.Add(MapUser(reader));
            return list;
        }

        public async Task<List<User>> GetAllUsersAsync() {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_get_all_users", connection);
            command.CommandType = CommandType.StoredProcedure;
            await connection.OpenAsync();
            var list = new List<User>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) list.Add(MapUser(reader));
            return list;
        }
    }
}

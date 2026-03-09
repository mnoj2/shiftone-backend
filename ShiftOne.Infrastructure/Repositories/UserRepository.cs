using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ShiftOne.Domain.Entities;
using ShiftOne.Domain.Interfaces;
using System.Data;

namespace ShiftOne.Infrastructure.Repositories {
    public class UserRepository : IUserRepository {

        private readonly string _connectionString;
        public UserRepository(IConfiguration configuration) {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        private User MapUser(SqlDataReader reader) {
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

            User? user = null;

            using(var reader = await command.ExecuteReaderAsync()) {
                if(await reader.ReadAsync()) {
                    user = new User {
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
            }

            string statusCode = (string) statusCodeParam.Value;
            if(statusCode == "F") {

            }

            return user;
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

            string statusCode = (string) statusCodeParam.Value;
            if(statusCode == "F") {

            }

            return statusCode == "S";
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

            if(await reader.ReadAsync()) {
                return MapUser(reader);
            }

            string statusCode = (string) statusCodeParam.Value;
            if(statusCode == "F") {

            }

            return null;
        }


    }
}

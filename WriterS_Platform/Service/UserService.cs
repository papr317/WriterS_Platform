using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Dapper;
using WriterS_Platform.Models;
using BCrypt.Net; // Добавляем using для BCrypt

namespace WriterS_Platform.Services // Объявление Namespace!
{
    public class UserService : IUser // Реализует ваш интерфейс
    {
        private readonly IConfiguration _config;
        private string ConnectionString => _config["db"]; // ConnectionString существует здесь

        // Конструктор, который инициализирует _config
        public UserService(IConfiguration configuration)
        {
            _config = configuration;
        }

        // --- 1. РЕГИСТРАЦИЯ ---
        public async Task<int> RegisterUserAsync(User user)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var parameters = new { user.NikeName, user.PasswordHASH, user.Email };

                // Используем хранимую процедуру
                var result = await connection.QuerySingleAsync<int>(
                    "pRegisterUser",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );
                return result;
            }
        }

        // --- 2. ПОИСК ДЛЯ ВХОДА ---
        public async Task<User> GetUserByLoginAsync(string identifier, string password)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var sql = "SELECT * FROM Users WHERE NikeName = @Identifier OR Email = @Identifier";

                var user = await connection.QuerySingleOrDefaultAsync<User>(
                    sql,
                    new { Identifier = identifier }
                );

                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHASH))
                {
                    return user;
                }
                return null;
            }
        }

        // --- 3. ПОЛУЧЕНИЕ ПРОФИЛЯ ---
        public async Task<User> GetProfileByIdAsync(int userId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                // Не выбираем HASH пароля!
                var sql = "SELECT id, NikeName, Email FROM Users WHERE id = @Id";
                return await connection.QuerySingleOrDefaultAsync<User>(
                    sql,
                    new { Id = userId }
                );
            }
        }

        // --- 4. ПОИСК ПО ID (для аутентификации) ---
        public async Task<User> FindUserByIdAsync(int id)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var sql = "SELECT id, NikeName, Email FROM Users WHERE id = @Id";

                return await connection.QuerySingleOrDefaultAsync<User>(
                    sql,
                    new { Id = id }
                );
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var sql = @"UPDATE Users
                              SET NikeName = @NikeName, Email = @Email, PasswordHASH = @PasswordHASH
                            WHERE id = @id";
                var rowsAffected = await connection.ExecuteAsync(sql, user);
                return rowsAffected > 0;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var sql = "DELETE FROM Users WHERE id = @Id";
                var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
                return rowsAffected > 0;
            }
        }
    }
}
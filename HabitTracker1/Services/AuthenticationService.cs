using System;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using HabitTracker1.Data;
using HabitTracker1.Models;

namespace HabitTracker1.Services
{
    public interface IAuthenticationService
    {
        User Register(string username, string password, string email = null);
        User Login(string username, string password);
        void Logout();
        User GetCurrentUser();
        void DeleteUser(int userId);
        bool UserExists(string username);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private User _currentUser;
        private readonly DatabaseConnection _dbConnection;

        public AuthenticationService()
        {
            _dbConnection = new DatabaseConnection();
        }

        public User Register(string username, string password, string email = null)
        {
            if (UserExists(username))
                throw new Exception("Пользователь с таким именем уже существует.");

            try
            {
                string passwordHash = HashPassword(password);
                string query = @"INSERT INTO Users (Username, PasswordHash, Email, CreatedDate) 
                               VALUES (@Username, @PasswordHash, @Email, @CreatedDate);
                               SELECT CAST(SCOPE_IDENTITY() as int)";

                SqlParameter[] parameters = new[]
                {
                    new SqlParameter("@Username", username),
                    new SqlParameter("@PasswordHash", passwordHash),
                    new SqlParameter("@Email", email ?? (object)DBNull.Value),
                    new SqlParameter("@CreatedDate", DateTime.Now)
                };

                object result = _dbConnection.ExecuteScalar(query, parameters);
                int userId = Convert.ToInt32(result);

                return new User
                {
                    Id = userId,
                    Username = username,
                    PasswordHash = passwordHash,
                    Email = email,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка при регистрации: {ex.Message}");
            }
        }

        public User Login(string username, string password)
        {
            try
            {
                string query = @"SELECT UserId as Id, Username, PasswordHash, Email, CreatedDate as CreatedAt 
                               FROM Users WHERE Username = @Username";

                SqlParameter[] parameters = new[] { new SqlParameter("@Username", username) };
                DataTable dt = _dbConnection.ExecuteQuery(query, parameters);

                if (dt.Rows.Count == 0)
                    throw new Exception("Пользователь не найден.");

                DataRow row = dt.Rows[0];
                string storedHash = row["PasswordHash"].ToString();

                if (!VerifyPassword(password, storedHash))
                    throw new Exception("Неверный пароль.");

                _currentUser = new User
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Username = row["Username"].ToString(),
                    PasswordHash = storedHash,
                    Email = row["Email"] == DBNull.Value ? null : row["Email"].ToString(),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                    IsActive = true
                };

                return _currentUser;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка при входе: {ex.Message}");
            }
        }

        public void Logout()
        {
            _currentUser = null;
        }

        public User GetCurrentUser()
        {
            return _currentUser;
        }

        public void DeleteUser(int userId)
        {
            try
            {
                string query = "DELETE FROM Users WHERE UserId = @UserId";
                SqlParameter[] parameters = new[] { new SqlParameter("@UserId", userId) };
                _dbConnection.ExecuteCommand(query, parameters);
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка при удалении пользователя: {ex.Message}");
            }
        }

        public bool UserExists(string username)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                SqlParameter[] parameters = new[] { new SqlParameter("@Username", username) };
                object result = _dbConnection.ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }
    }
}


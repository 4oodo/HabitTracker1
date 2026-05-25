using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HabitTracker1.Data;
using HabitTracker1.Models;

namespace HabitTracker1.Services
{
    public interface ICategoryService
    {
        List<Category> GetUserCategories(int userId);
        Category CreateCategory(int userId, string name, string color = null);
        void UpdateCategory(int categoryId, string name, string color = null);
        void DeleteCategory(int categoryId);
        void MoveHabitsToNone(int categoryId);
    }

    public class CategoryService : ICategoryService
    {
        private readonly DatabaseConnection _dbConnection;

        public CategoryService()
        {
            _dbConnection = new DatabaseConnection();
        }

        public List<Category> GetUserCategories(int userId)
        {
            var categories = new List<Category>();

            try
            {
                string query = @"SELECT CategoryId as Id, Name, UserId, CreatedDate as CreatedAt 
                               FROM Categories WHERE UserId = @UserId ORDER BY Name";
                SqlParameter[] parameters = new[] { new SqlParameter("@UserId", userId) };
                DataTable dt = _dbConnection.ExecuteQuery(query, parameters);

                foreach (DataRow row in dt.Rows)
                {
                    categories.Add(new Category
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Name = row["Name"].ToString(),
                        Color = null, // Color поле не существует в БД
                        UserId = Convert.ToInt32(row["UserId"]),
                        CreatedAt = Convert.ToDateTime(row["CreatedAt"])
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка при получении категорий: {ex.Message}");
            }

            return categories;
        }

        public Category CreateCategory(int userId, string name, string color = null)
        {
            try
            {
                string query = @"INSERT INTO Categories (Name, UserId, CreatedDate) 
                               VALUES (@Name, @UserId, @CreatedDate);
                               SELECT CAST(SCOPE_IDENTITY() as int)";

                SqlParameter[] parameters = new[]
                {
                    new SqlParameter("@Name", name),
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@CreatedDate", DateTime.Now)
                };

                object result = _dbConnection.ExecuteScalar(query, parameters);
                int categoryId = Convert.ToInt32(result);

                return new Category
                {
                    Id = categoryId,
                    Name = name,
                    Color = null,
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка при создании категории: {ex.Message}");
            }
        }

        public void UpdateCategory(int categoryId, string name, string color = null)
        {
            try
            {
                string query = "UPDATE Categories SET Name = @Name WHERE CategoryId = @Id";
                SqlParameter[] parameters = new[]
                {
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Id", categoryId)
                };
                _dbConnection.ExecuteCommand(query, parameters);
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка при обновлении категории: {ex.Message}");
            }
        }

        public void DeleteCategory(int categoryId)
        {
            try
            {
                MoveHabitsToNone(categoryId);

                string query = "DELETE FROM Categories WHERE CategoryId = @Id";
                SqlParameter[] parameters = new[] { new SqlParameter("@Id", categoryId) };
                _dbConnection.ExecuteCommand(query, parameters);
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка при удалении категории: {ex.Message}");
            }
        }

        public void MoveHabitsToNone(int categoryId)
        {
            try
            {
                string query = "UPDATE Habits SET CategoryId = NULL WHERE CategoryId = @CategoryId";
                SqlParameter[] parameters = new[] { new SqlParameter("@CategoryId", categoryId) };
                _dbConnection.ExecuteCommand(query, parameters);
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка при переводе привычек: {ex.Message}");
            }
        }
    }
}

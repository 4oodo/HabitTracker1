using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HabitTracker1.Data;
using HabitTracker1.Models;

namespace HabitTracker1.Services
{
    public interface IHabitService
    {
        List<Habit> GetUserHabits(int userId, bool includeInactive = false);
        Habit CreateHabit(int userId, string title, int? categoryId, FrequencyType frequency, 
            int frequencyDays = 1, string unit = null, decimal? targetValue = null, TimeSpan? reminderTime = null, bool reminderEnabled = false, string description = null);
        void UpdateHabit(int habitId, string title, int? categoryId, FrequencyType frequency, 
            int frequencyDays = 1, string unit = null, decimal? targetValue = null, TimeSpan? reminderTime = null, bool reminderEnabled = false, string description = null);
        void DeleteHabit(int habitId);
        Habit GetHabitById(int habitId);
        void LogHabitCompletion(int habitId, DateTime date, bool isCompleted, decimal? value = null, string note = null);
        List<HabitLog> GetHabitLogsByDateRange(int habitId, DateTime startDate, DateTime endDate);
    }

    public class HabitService : IHabitService
    {
        private readonly DatabaseConnection _dbConnection;

        public HabitService()
        {
            _dbConnection = new DatabaseConnection();
        }

        public List<Habit> GetUserHabits(int userId, bool includeInactive = false)
        {
            var habits = new List<Habit>();

            try
            {
                string whereClause = "WHERE h.UserId = @UserId AND h.DeletedDate IS NULL";
                if (!includeInactive)
                    whereClause += " AND h.IsActive = 1";

                string query = $@"SELECT h.HabitId as Id, h.Name as Title, h.Description, h.UserId, h.CategoryId, h.Frequency,
                               h.FrequencyDays, h.Unit, h.TargetValue, h.ReminderTime, h.IsActive,
                               h.CreatedDate as CreatedAt, h.DeletedDate as DeletedAt, c.Name as CategoryName
                               FROM Habits h
                               LEFT JOIN Categories c ON h.CategoryId = c.CategoryId
                               {whereClause}
                               ORDER BY h.Name";

                SqlParameter[] parameters = new[] { new SqlParameter("@UserId", userId) };
                DataTable dt = _dbConnection.ExecuteQuery(query, parameters);

                if (dt == null || dt.Rows.Count == 0)
                    return habits;

                foreach (DataRow row in dt.Rows)
                {
                    TimeSpan? reminderTime = null;
                    if (row["ReminderTime"] != DBNull.Value)
                    {
                        object timeValue = row["ReminderTime"];
                        if (timeValue is TimeSpan)
                            reminderTime = (TimeSpan)timeValue;
                        else
                            reminderTime = TimeSpan.Parse(timeValue.ToString());
                    }

                    var habit = new Habit
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Title = row["Title"].ToString(),
                        Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString(),
                        UserId = Convert.ToInt32(row["UserId"]),
                        CategoryId = row["CategoryId"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CategoryId"]),
                        Frequency = (FrequencyType)Convert.ToInt32(row["Frequency"]),
                        FrequencyDays = Convert.ToInt32(row["FrequencyDays"]),
                        Unit = row["Unit"] == DBNull.Value ? null : row["Unit"].ToString(),
                        TargetValue = row["TargetValue"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["TargetValue"]),
                        ReminderTime = reminderTime,
                        ReminderEnabled = false,
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                        DeletedAt = row["DeletedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["DeletedAt"]),
                        Category = row["CategoryName"] == DBNull.Value ? null : new Category { Name = row["CategoryName"].ToString() }
                    };
                    habits.Add(habit);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка при получении привычек: {ex.Message}");
            }

            return habits;
        }

        public Habit CreateHabit(int userId, string title, int? categoryId, FrequencyType frequency, 
            int frequencyDays = 1, string unit = null, decimal? targetValue = null, TimeSpan? reminderTime = null, bool reminderEnabled = false, string description = null)
        {
            try
            {
                string query = @"INSERT INTO Habits (Name, Description, UserId, CategoryId, Frequency, FrequencyDays,
                               Unit, TargetValue, ReminderTime, IsActive, CreatedDate)
                               VALUES (@Title, @Description, @UserId, @CategoryId, @Frequency, @FrequencyDays,
                               @Unit, @TargetValue, @ReminderTime, 1, @CreatedDate);
                               SELECT CAST(SCOPE_IDENTITY() as int)";

                SqlParameter[] parameters = new[]
                {
                    new SqlParameter("@Title", title),
                    new SqlParameter("@Description", description ?? (object)DBNull.Value),
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@CategoryId", categoryId ?? (object)DBNull.Value),
                    new SqlParameter("@Frequency", (int)frequency),
                    new SqlParameter("@FrequencyDays", frequencyDays),
                    new SqlParameter("@Unit", unit ?? (object)DBNull.Value),
                    new SqlParameter("@TargetValue", targetValue ?? (object)DBNull.Value),
                    new SqlParameter("@ReminderTime", reminderTime.HasValue ? (object)reminderTime.Value : DBNull.Value),
                    new SqlParameter("@CreatedDate", DateTime.Now)
                };

                object result = _dbConnection.ExecuteScalar(query, parameters);
                int habitId = Convert.ToInt32(result);

                return new Habit
                {
                    Id = habitId,
                    Title = title,
                    Description = description,
                    UserId = userId,
                    CategoryId = categoryId,
                    Frequency = frequency,
                    FrequencyDays = frequencyDays,
                    Unit = unit,
                    TargetValue = targetValue,
                    ReminderTime = reminderTime,
                    ReminderEnabled = reminderEnabled,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка при создании привычки: {ex.Message}");
            }
        }

        public void UpdateHabit(int habitId, string title, int? categoryId, FrequencyType frequency,
            int frequencyDays = 1, string unit = null, decimal? targetValue = null, TimeSpan? reminderTime = null, bool reminderEnabled = false, string description = null)
        {
            try
            {
                string query = @"UPDATE Habits SET Name = @Title, Description = @Description, CategoryId = @CategoryId, 
                               Frequency = @Frequency, FrequencyDays = @FrequencyDays, Unit = @Unit,
                               TargetValue = @TargetValue, ReminderTime = @ReminderTime
                               WHERE HabitId = @Id";

                SqlParameter[] parameters = new[]
                {
                    new SqlParameter("@Title", title),
                    new SqlParameter("@Description", description ?? (object)DBNull.Value),
                    new SqlParameter("@CategoryId", categoryId ?? (object)DBNull.Value),
                    new SqlParameter("@Frequency", (int)frequency),
                    new SqlParameter("@FrequencyDays", frequencyDays),
                    new SqlParameter("@Unit", unit ?? (object)DBNull.Value),
                    new SqlParameter("@TargetValue", targetValue ?? (object)DBNull.Value),
                    new SqlParameter("@ReminderTime", reminderTime.HasValue ? (object)reminderTime.Value : DBNull.Value),
                    new SqlParameter("@Id", habitId)
                };

                _dbConnection.ExecuteCommand(query, parameters);
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка при обновлении привычки: {ex.Message}");
            }
        }

        public void DeleteHabit(int habitId)
        {
            try
            {
                string query = "UPDATE Habits SET DeletedDate = @DeletedDate, IsActive = 0 WHERE HabitId = @Id";
                SqlParameter[] parameters = new[]
                {
                    new SqlParameter("@DeletedDate", DateTime.Now),
                    new SqlParameter("@Id", habitId)
                };
                _dbConnection.ExecuteCommand(query, parameters);
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка при удалении привычки: {ex.Message}");
            }
        }

        public Habit GetHabitById(int habitId)
        {
            try
            {
                string query = @"SELECT HabitId as Id, Name as Title, Description, UserId, CategoryId, Frequency,
                               FrequencyDays, Unit, TargetValue, ReminderTime, IsActive,
                               CreatedDate as CreatedAt, DeletedDate as DeletedAt FROM Habits WHERE HabitId = @Id";

                SqlParameter[] parameters = new[] { new SqlParameter("@Id", habitId) };
                DataTable dt = _dbConnection.ExecuteQuery(query, parameters);

                if (dt == null || dt.Rows.Count == 0)
                    return null;

                DataRow row = dt.Rows[0];
                TimeSpan? reminderTime = null;
                if (row["ReminderTime"] != DBNull.Value)
                {
                    object timeValue = row["ReminderTime"];
                    if (timeValue is TimeSpan)
                        reminderTime = (TimeSpan)timeValue;
                    else
                        reminderTime = TimeSpan.Parse(timeValue.ToString());
                }

                return new Habit
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Title = row["Title"].ToString(),
                    Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString(),
                    UserId = Convert.ToInt32(row["UserId"]),
                    CategoryId = row["CategoryId"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CategoryId"]),
                    Frequency = (FrequencyType)Convert.ToInt32(row["Frequency"]),
                    FrequencyDays = Convert.ToInt32(row["FrequencyDays"]),
                    Unit = row["Unit"] == DBNull.Value ? null : row["Unit"].ToString(),
                    TargetValue = row["TargetValue"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["TargetValue"]),
                    ReminderTime = reminderTime,
                    ReminderEnabled = false,
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                    DeletedAt = row["DeletedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["DeletedAt"])
                };
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка при получении привычки: {ex.Message}");
            }
        }

        public void LogHabitCompletion(int habitId, DateTime date, bool isCompleted, decimal? value = null, string note = null)
        {
            try
            {
                var habit = GetHabitById(habitId);
                if (habit == null)
                    throw new Exception("Привычка не найдена.");

                if ((DateTime.Now - date).TotalDays > 7)
                    throw new Exception("Можно отметить выполнение только за последние 7 дней.");

                // Проверяем наличие записи за эту дату
                string checkQuery = @"SELECT LogId FROM HabitLogs 
                                     WHERE HabitId = @HabitId AND CAST(LogDate as date) = CAST(@Date as date)";
                SqlParameter[] checkParams = new[]
                {
                    new SqlParameter("@HabitId", habitId),
                    new SqlParameter("@Date", date.Date)
                };

                try
                {
                    object existingLogId = _dbConnection.ExecuteScalar(checkQuery, checkParams);

                    if (existingLogId != null && existingLogId != DBNull.Value)
                    {
                        // Обновляем существующую запись
                        string updateQuery = @"UPDATE HabitLogs SET IsCompleted = @IsCompleted, Value = @Value, Note = @Note
                                              WHERE LogId = @Id";
                        SqlParameter[] updateParams = new[]
                        {
                            new SqlParameter("@IsCompleted", isCompleted),
                            new SqlParameter("@Value", value ?? (object)DBNull.Value),
                            new SqlParameter("@Note", note ?? (object)DBNull.Value),
                            new SqlParameter("@Id", Convert.ToInt32(existingLogId))
                        };
                        _dbConnection.ExecuteCommand(updateQuery, updateParams);
                    }
                    else
                    {
                        // Создаём новую запись
                        string insertQuery = @"INSERT INTO HabitLogs (HabitId, UserId, LogDate, IsCompleted, Value, Note, CreatedDate)
                                              VALUES (@HabitId, @UserId, @Date, @IsCompleted, @Value, @Note, @CreatedDate)";
                        SqlParameter[] insertParams = new[]
                        {
                            new SqlParameter("@HabitId", habitId),
                            new SqlParameter("@UserId", habit.UserId),
                            new SqlParameter("@Date", date.Date),
                            new SqlParameter("@IsCompleted", isCompleted),
                            new SqlParameter("@Value", value ?? (object)DBNull.Value),
                            new SqlParameter("@Note", note ?? (object)DBNull.Value),
                            new SqlParameter("@CreatedDate", DateTime.Now)
                        };
                        _dbConnection.ExecuteCommand(insertQuery, insertParams);
                    }
                }
                catch (Exception dbEx)
                {
                    throw new Exception($"Ошибка при работе с базой данных: {dbEx.Message}", dbEx);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка при логировании привычки (SQL): {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при логировании привычки: {ex.Message}", ex);
            }
        }

        public List<HabitLog> GetHabitLogsByDateRange(int habitId, DateTime startDate, DateTime endDate)
        {
            var logs = new List<HabitLog>();

            try
            {
                string query = @"SELECT LogId as Id, HabitId, UserId, LogDate as Date, IsCompleted, Value, Note, CreatedDate as CreatedAt
                               FROM HabitLogs 
                               WHERE HabitId = @HabitId AND LogDate >= @StartDate AND LogDate <= @EndDate
                               ORDER BY LogDate ASC";

                SqlParameter[] parameters = new[]
                {
                    new SqlParameter("@HabitId", habitId),
                    new SqlParameter("@StartDate", startDate.Date),
                    new SqlParameter("@EndDate", endDate.Date)
                };

                DataTable dt = _dbConnection.ExecuteQuery(query, parameters);

                foreach (DataRow row in dt.Rows)
                {
                    logs.Add(new HabitLog
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        HabitId = Convert.ToInt32(row["HabitId"]),
                        UserId = Convert.ToInt32(row["UserId"]),
                        Date = Convert.ToDateTime(row["Date"]),
                        IsCompleted = Convert.ToBoolean(row["IsCompleted"]),
                        Value = row["Value"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["Value"]),
                        Notes = row["Note"] == DBNull.Value ? null : row["Note"].ToString(),
                        CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                        ModifiedAt = null // ModifiedAt не существует в БД
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Ошибка при получении логов: {ex.Message}");
            }

            return logs;
        }
    }
}

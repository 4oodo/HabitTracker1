using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using HabitTracker1.Data;
using HabitTracker1.Models;

namespace HabitTracker1.Services
{
    public class StatisticsData
    {
        public decimal SuccessPercentage { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalExpected { get; set; }
    }

    public interface IStatisticsService
    {
        StatisticsData GetHabitStatistics(int habitId, DateTime startDate, DateTime endDate);
        List<StatisticsData> GetCategoryStatistics(int categoryId, DateTime startDate, DateTime endDate);
        List<StatisticsData> GetAllUserStatistics(int userId, DateTime startDate, DateTime endDate);
        List<HabitLog> GetHeatmapData(int habitId, int year, int month);
        Dictionary<string, decimal> GetCategoryPercentages(int userId, DateTime startDate, DateTime endDate);
    }

    public class StatisticsService : IStatisticsService
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly IHabitService _habitService;

        public StatisticsService()
        {
            _dbConnection = new DatabaseConnection();
            _habitService = new HabitService();
        }

        public StatisticsData GetHabitStatistics(int habitId, DateTime startDate, DateTime endDate)
        {
            try
            {
                string query = @"SELECT Id, IsCompleted FROM HabitLogs 
                               WHERE HabitId = @HabitId AND LogDate >= @StartDate AND LogDate <= @EndDate
                               ORDER BY LogDate ASC";

                SqlParameter[] parameters = new[]
                {
                    new SqlParameter("@HabitId", habitId),
                    new SqlParameter("@StartDate", startDate.Date),
                    new SqlParameter("@EndDate", endDate.Date)
                };

                DataTable dt = _dbConnection.ExecuteQuery(query, parameters);
                var logs = new List<HabitLog>();

                foreach (DataRow row in dt.Rows)
                {
                    logs.Add(new HabitLog
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        IsCompleted = Convert.ToBoolean(row["IsCompleted"])
                    });
                }

                var habit = _habitService.GetHabitById(habitId);
                int totalCompleted = logs.Count(l => l.IsCompleted);
                int totalExpected = CalculateExpectedCount(habit, startDate, endDate);

                return new StatisticsData
                {
                    TotalCompleted = totalCompleted,
                    TotalExpected = totalExpected,
                    SuccessPercentage = totalExpected > 0 ? (decimal)totalCompleted / totalExpected * 100 : 0,
                    CurrentStreak = CalculateCurrentStreak(logs),
                    LongestStreak = CalculateLongestStreak(logs)
                };
            }
            catch
            {
                return new StatisticsData();
            }
        }

        public List<StatisticsData> GetCategoryStatistics(int categoryId, DateTime startDate, DateTime endDate)
        {
            try
            {
                string query = @"SELECT Id FROM Habits 
                               WHERE CategoryId = @CategoryId AND DeletedAt IS NULL";

                SqlParameter[] parameters = new[] { new SqlParameter("@CategoryId", categoryId) };
                DataTable dt = _dbConnection.ExecuteQuery(query, parameters);

                var results = new List<StatisticsData>();
                foreach (DataRow row in dt.Rows)
                {
                    int habitId = Convert.ToInt32(row["Id"]);
                    results.Add(GetHabitStatistics(habitId, startDate, endDate));
                }

                return results;
            }
            catch
            {
                return new List<StatisticsData>();
            }
        }

        public List<StatisticsData> GetAllUserStatistics(int userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                string query = @"SELECT Id FROM Habits 
                               WHERE UserId = @UserId AND DeletedAt IS NULL AND IsActive = 1";

                SqlParameter[] parameters = new[] { new SqlParameter("@UserId", userId) };
                DataTable dt = _dbConnection.ExecuteQuery(query, parameters);

                var results = new List<StatisticsData>();
                foreach (DataRow row in dt.Rows)
                {
                    int habitId = Convert.ToInt32(row["Id"]);
                    results.Add(GetHabitStatistics(habitId, startDate, endDate));
                }

                return results;
            }
            catch
            {
                return new List<StatisticsData>();
            }
        }

        public List<HabitLog> GetHeatmapData(int habitId, int year, int month)
        {
            var logs = new List<HabitLog>();

            try
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                string query = @"SELECT Id, HabitId, UserId, Date, IsCompleted, Value, Notes, CreatedAt, ModifiedAt
                               FROM HabitLogs 
                               WHERE HabitId = @HabitId AND Date >= @StartDate AND Date <= @EndDate
                               ORDER BY Date ASC";

                SqlParameter[] parameters = new[]
                {
                    new SqlParameter("@HabitId", habitId),
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate)
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
                        Notes = row["Notes"] == DBNull.Value ? null : row["Notes"].ToString(),
                        CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                        ModifiedAt = row["ModifiedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ModifiedAt"])
                    });
                }
            }
            catch
            {
                // Возвращаем пустой список при ошибке
            }

            return logs;
        }

        public Dictionary<string, decimal> GetCategoryPercentages(int userId, DateTime startDate, DateTime endDate)
        {
            var result = new Dictionary<string, decimal>();

            try
            {
                // Получаем все категории пользователя
                string categoriesQuery = @"SELECT Id, Name FROM Categories WHERE UserId = @UserId";
                SqlParameter[] params1 = new[] { new SqlParameter("@UserId", userId) };
                DataTable categoriesTable = _dbConnection.ExecuteQuery(categoriesQuery, params1);

                foreach (DataRow catRow in categoriesTable.Rows)
                {
                    int categoryId = Convert.ToInt32(catRow["Id"]);
                    string categoryName = catRow["Name"].ToString();

                    var stats = GetCategoryStatistics(categoryId, startDate, endDate);
                    decimal percentage = stats.Count > 0 ? (decimal)stats.Average(s => s.SuccessPercentage) : 0;
                    result[categoryName] = percentage;
                }

                // Привычки без категории
                string noCategoryQuery = @"SELECT Id FROM Habits 
                                          WHERE UserId = @UserId AND CategoryId IS NULL AND DeletedAt IS NULL AND IsActive = 1";
                DataTable noCategoryTable = _dbConnection.ExecuteQuery(noCategoryQuery, params1);

                if (noCategoryTable.Rows.Count > 0)
                {
                    var noCategoryStats = new List<StatisticsData>();
                    foreach (DataRow row in noCategoryTable.Rows)
                    {
                        int habitId = Convert.ToInt32(row["Id"]);
                        noCategoryStats.Add(GetHabitStatistics(habitId, startDate, endDate));
                    }

                    decimal percentage = noCategoryStats.Count > 0 ? (decimal)noCategoryStats.Average(s => s.SuccessPercentage) : 0;
                    result["Без категории"] = percentage;
                }
            }
            catch
            {
                // Возвращаем пустой словарь при ошибке
            }

            return result;
        }

        private int CalculateExpectedCount(Habit habit, DateTime startDate, DateTime endDate)
        {
            int count = 0;
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                if (ShouldHabitBePerformed(habit, currentDate))
                    count++;

                currentDate = currentDate.AddDays(1);
            }

            return count;
        }

        private bool ShouldHabitBePerformed(Habit habit, DateTime date)
        {
            switch (habit.Frequency)
            {
                case FrequencyType.Daily:
                    return true;

                case FrequencyType.Weekdays:
                    return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;

                case FrequencyType.Weekly:
                    return true;

                case FrequencyType.EveryNDays:
                    return true;

                case FrequencyType.SpecificDays:
                    // TODO: Implement specific days logic when DaysOfWeek is available
                    return true;

                default:
                    return false;
            }
        }

        private int CalculateCurrentStreak(List<HabitLog> logs)
        {
            if (logs.Count == 0 || !logs.Last().IsCompleted)
                return 0;

            int streak = 0;
            for (int i = logs.Count - 1; i >= 0; i--)
            {
                if (logs[i].IsCompleted)
                    streak++;
                else
                    break;
            }

            return streak;
        }

        private int CalculateLongestStreak(List<HabitLog> logs)
        {
            int maxStreak = 0;
            int currentStreak = 0;

            foreach (var log in logs)
            {
                if (log.IsCompleted)
                {
                    currentStreak++;
                    if (currentStreak > maxStreak)
                        maxStreak = currentStreak;
                }
                else
                {
                    currentStreak = 0;
                }
            }

            return maxStreak;
        }
    }
}

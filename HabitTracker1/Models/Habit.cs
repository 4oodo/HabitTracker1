using System;
using System.Collections.Generic;

namespace HabitTracker1.Models
{
    public class Habit
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
        public int? CategoryId { get; set; }
        public FrequencyType Frequency { get; set; }
        public int FrequencyDays { get; set; }
        public string Unit { get; set; }
        public decimal? TargetValue { get; set; }
        public TimeSpan? ReminderTime { get; set; }
        public bool ReminderEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? DeletedAt { get; set; }

        public virtual User User { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<HabitLog> HabitLogs { get; set; }
    }

    public enum FrequencyType
    {
        Daily = 0,
        Weekdays = 1,
        SpecificDays = 2,
        EveryNDays = 3,
        Weekly = 4
    }
}


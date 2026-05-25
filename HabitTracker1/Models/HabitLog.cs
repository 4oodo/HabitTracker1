using System;

namespace HabitTracker1.Models
{
    public class HabitLog
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public bool IsCompleted { get; set; }
        public decimal? Value { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public virtual Habit Habit { get; set; }
        public virtual User User { get; set; }
    }
}


using System;
using System.Collections.Generic;

namespace HabitTracker1.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<Habit> Habits { get; set; }
        public virtual ICollection<HabitLog> HabitLogs { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
    }
}


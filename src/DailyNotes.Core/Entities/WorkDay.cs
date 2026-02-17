using System;
using System.Collections.Generic;
using DailyNotes.Core.Interfaces;

namespace DailyNotes.Core.Entities
{
    public class WorkDay : IHasTenantUser
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateOnly WorkDate { get; set; }
        public TimeOnly? TimeIn1 { get; set; }
        public TimeOnly? TimeOut1 { get; set; }
        public TimeOnly? TimeIn2 { get; set; }
        public TimeOnly? TimeOut2 { get; set; }
        public TimeOnly? TimeIn3 { get; set; }
        public TimeOnly? TimeOut3 { get; set; }
        public int BreakMinutes { get; set; } = 0;
        public string? Comments { get; set; }

        // Computed
        public double HoursWorked
        {
            get
            {
                double total = 0;
                if (TimeIn1.HasValue && TimeOut1.HasValue) total += (TimeOut1.Value - TimeIn1.Value).TotalHours;
                if (TimeIn2.HasValue && TimeOut2.HasValue) total += (TimeOut2.Value - TimeIn2.Value).TotalHours;
                if (TimeIn3.HasValue && TimeOut3.HasValue) total += (TimeOut3.Value - TimeIn3.Value).TotalHours;
                return total - (BreakMinutes / 60.0);
            }
        }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<WorkNote> Notes { get; set; } = new List<WorkNote>();
    }
}

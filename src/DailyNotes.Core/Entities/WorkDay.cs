using System;
using System.Collections.Generic;

namespace DailyNotes.Core.Entities
{
    public class WorkDay
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime? TimeIn1 { get; set; }
        public DateTime? TimeOut1 { get; set; }
        public DateTime? TimeIn2 { get; set; }
        public DateTime? TimeOut2 { get; set; }
        public DateTime? TimeIn3 { get; set; }
        public DateTime? TimeOut3 { get; set; }
        public int BreakMinutes { get; set; } = 0;

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

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
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public int BreakMinutes { get; set; } = 0;

        // Computed
        public double HoursWorked =>
            (TimeIn.HasValue && TimeOut.HasValue)
            ? (TimeOut.Value - TimeIn.Value).TotalHours - (BreakMinutes / 60.0)
            : 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<WorkNote> Notes { get; set; } = new List<WorkNote>();
    }
}

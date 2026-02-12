using System;

namespace DailyNotes.Core.Entities
{
    public class Assignment
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int CourseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = "pending"; // 'pending' | 'submitted' | 'graded'

        public decimal? Points { get; set; }        // e.g. 95
        public decimal? MaxPoints { get; set; }     // e.g. 100
        public decimal? Weight { get; set; }        // e.g. 0.20 (20% of grade)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

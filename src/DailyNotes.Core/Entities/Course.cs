using System;
using System.Collections.Generic;

namespace DailyNotes.Core.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty; // e.g., "Fall 2024"
        public string Name { get; set; } = string.Empty;     // e.g., "CS 101"
        public string? Instructor { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Credits { get; set; } = 3;

        // Grade tracking
        public decimal? TargetGrade { get; set; } // e.g. 4.0 or 95.0
        public decimal? CurrentGrade { get; set; }

        // External
        public string? ExternalSource { get; set; } // 'udemy', 'coursera', etc.
        public string? ExternalId { get; set; }
        public string? ExternalUrl { get; set; }
        public int ProgressPercent { get; set; } = 0;

        public int? TopicId { get; set; } // Linked KB topic

        public bool IsPinned { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}

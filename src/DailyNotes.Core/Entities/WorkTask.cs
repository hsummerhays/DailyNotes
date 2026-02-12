using System;

namespace DailyNotes.Core.Entities
{
    public class WorkTask
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Visibilty { get; set; } = "private";
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "pending"; // 'pending' | 'in_progress' | 'completed' | 'on_hold'
        public DateTime? DueDate { get; set; }
        public int? ProjectId { get; set; }
        public int? ParentTaskId { get; set; }

        // External
        public string? ExternalSource { get; set; }
        public string? ExternalId { get; set; }

        public bool IsPinned { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

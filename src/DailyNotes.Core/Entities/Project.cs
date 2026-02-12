using System;

namespace DailyNotes.Core.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Visibility { get; set; } = "private";
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public DateOnly? CreatedDate { get; set; }
        public DateOnly? CompletedDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

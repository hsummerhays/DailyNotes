using System;
using System.Text.Json;

namespace DailyNotes.Core.Entities
{
    public class WorkNote
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Visibility { get; set; } = "private";
        public int WorkDayId { get; set; }
        public int? TaskId { get; set; }
        public JsonDocument Content { get; set; } = JsonDocument.Parse("{}");
        public int TimeMinutes { get; set; } = 0;

        public bool IsPinned { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

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
        public int? WorkTaskId { get; set; }
        public DateTime NoteDate { get; set; }
        public JsonDocument Content { get; set; } = JsonDocument.Parse("{}");
        public int TimeMinutes { get; set; } = 0;

        // External integration
        public string? ExternalSource { get; set; }
        public string? ExternalId { get; set; }

        public bool IsPinned { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

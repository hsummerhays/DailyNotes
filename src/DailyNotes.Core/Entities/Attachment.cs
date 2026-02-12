using System;

namespace DailyNotes.Core.Entities
{
    public class Attachment
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty; // 'work_note' | 'work_task' | 'project'
        public int ItemId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
        public long? FileSizeBytes { get; set; }
        public string Source { get; set; } = "upload"; // 'upload' | 'google_drive' | 'onedrive'
        public string? ExternalUrl { get; set; }
        public string? OcrText { get; set; }
        public string? Transcription { get; set; }
        public int? DurationSeconds { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

using System.ComponentModel.DataAnnotations;

namespace DailyNotes.Application.DTOs.Requests
{
    public class AttachmentRequest
    {
        [Required]
        [MaxLength(100)]
        public string ItemType { get; set; } = string.Empty;
        [Required]
        public int ItemId { get; set; }
        [Required]
        [MaxLength(500)]
        public string FileName { get; set; } = string.Empty;
        [Required]
        [MaxLength(255)]
        public string ContentType { get; set; } = string.Empty;
        [Required]
        [MaxLength(2000)]
        public string StoragePath { get; set; } = string.Empty;
        public long? FileSizeBytes { get; set; }
        [MaxLength(50)]
        public string Source { get; set; } = "upload";
        [MaxLength(2000)]
        public string? ExternalUrl { get; set; }
        public string? OcrText { get; set; }
        public string? Transcription { get; set; }
        public int? DurationSeconds { get; set; }
    }
}

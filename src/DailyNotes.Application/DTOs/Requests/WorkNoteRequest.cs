using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DailyNotes.Application.DTOs.Requests
{
    public class WorkNoteRequest
    {
        [Required]
        public DateOnly NoteDate { get; set; }
        public JsonDocument Content { get; set; } = JsonDocument.Parse("{}");
        public int? WorkTaskId { get; set; }
        [Range(0, 1440)]
        public int TimeMinutes { get; set; } = 0;
        [MaxLength(100)]
        public string? ExternalSource { get; set; }
        [MaxLength(255)]
        public string? ExternalId { get; set; }
        public bool IsPinned { get; set; } = false;
        [MaxLength(50)]
        public string Visibility { get; set; } = "private";
    }
}

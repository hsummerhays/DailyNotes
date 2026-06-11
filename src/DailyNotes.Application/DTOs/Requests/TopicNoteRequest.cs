using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DailyNotes.Application.DTOs.Requests
{
    public class TopicNoteRequest
    {
        [Required]
        public int TopicId { get; set; }
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;
        public JsonDocument Content { get; set; } = JsonDocument.Parse("{}");
        [Range(0, 1440)]
        public int TimeMinutes { get; set; } = 0;
        [MaxLength(50)]
        public string Visibility { get; set; } = "private";
    }
}

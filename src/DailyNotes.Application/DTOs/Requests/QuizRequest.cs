using System.ComponentModel.DataAnnotations;

namespace DailyNotes.Application.DTOs.Requests
{
    public class QuizRequest
    {
        [Required]
        public int TopicId { get; set; }
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;
        [Range(1, 5)]
        public int Difficulty { get; set; } = 1;
    }
}

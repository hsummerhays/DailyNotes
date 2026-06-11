using System.ComponentModel.DataAnnotations;

namespace DailyNotes.Application.DTOs.Requests
{
    public class QuizQuestionRequest
    {
        [Required]
        [MaxLength(2000)]
        public string QuestionText { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string? Explanation { get; set; }
        public int SortOrder { get; set; } = 0;
    }
}

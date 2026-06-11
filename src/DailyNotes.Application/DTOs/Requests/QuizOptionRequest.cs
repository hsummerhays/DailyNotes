using System.ComponentModel.DataAnnotations;

namespace DailyNotes.Application.DTOs.Requests
{
    public class QuizOptionRequest
    {
        [Required]
        [MaxLength(2000)]
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; } = false;
        public int SortOrder { get; set; } = 0;
    }
}

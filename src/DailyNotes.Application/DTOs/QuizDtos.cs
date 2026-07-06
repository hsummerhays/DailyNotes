using System.ComponentModel.DataAnnotations;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.DTOs
{
    public class QuizSubmissionDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "QuizId must be a positive integer.")]
        public int QuizId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one answer is required.")]
        public List<QuizAnswerDto> Answers { get; set; } = new();
    }

    public class QuizAnswerDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int QuestionId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int SelectedOptionId { get; set; }
    }

    public class QuizDetailDto
    {
        public Quiz Quiz { get; set; } = null!;
        public IEnumerable<QuizQuestionDetailDto> Questions { get; set; } = [];
    }

    public class QuizQuestionDetailDto
    {
        public QuizQuestion Question { get; set; } = null!;
        public IEnumerable<QuizOption> Options { get; set; } = [];
    }

    public class QuizAttemptDetailDto
    {
        public QuizAttempt Attempt { get; set; } = null!;
        public IEnumerable<QuizAnswer> Answers { get; set; } = [];
    }
}

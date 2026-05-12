using DailyNotes.Core.Entities;

namespace DailyNotes.Application.DTOs
{
    public class QuizSubmissionDto
    {
        public int QuizId { get; set; }
        public List<QuizAnswerDto> Answers { get; set; } = new();
    }

    public class QuizAnswerDto
    {
        public int QuestionId { get; set; }
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

using System;

namespace DailyNotes.Core.Entities
{
    public class QuizAnswer
    {
        public int AttemptId { get; set; }
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; }
        public bool IsCorrect { get; set; }
    }
}

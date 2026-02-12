using System;

namespace DailyNotes.Core.Entities
{
    public class QuizQuestion
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public int SortOrder { get; set; } = 0;
    }
}

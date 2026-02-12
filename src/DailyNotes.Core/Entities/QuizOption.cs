using System;

namespace DailyNotes.Core.Entities
{
    public class QuizOption
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; } = false;
        public int SortOrder { get; set; } = 0;
    }
}

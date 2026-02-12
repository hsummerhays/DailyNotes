using System;

namespace DailyNotes.Core.Entities
{
    public class Quiz
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int TopicId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Difficulty { get; set; } = 1; // 1-5
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

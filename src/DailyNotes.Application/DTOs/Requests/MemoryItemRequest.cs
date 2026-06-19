using System;

namespace DailyNotes.Application.DTOs.Requests
{
    public class MemoryItemRequest
    {
        public string MemoryType { get; set; } = string.Empty; // Conversation, Preference, Project, Person, Goal, Decision, Fact, Document, Task, Learning
        public string MemoryStatus { get; set; } = "Active";    // Active, Archived, Superseded, Incorrect
        public string Summary { get; set; } = string.Empty;
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public double ImportanceScore { get; set; }
        public double ConfidenceScore { get; set; }
        public int AccessCount { get; set; }
        public DateTime? LastConfirmedAt { get; set; }

        public int? RelatedMemoryId { get; set; }
        public string? SourceEntityType { get; set; }
        public int? SourceEntityId { get; set; }
        public string? SourceExcerpt { get; set; }
    }
}

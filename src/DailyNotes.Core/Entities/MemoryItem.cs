using System;
using DailyNotes.Core.Interfaces;

namespace DailyNotes.Core.Entities
{
    public class MemoryItem : IHasTenantUser
    {
        public const int EmbeddingDimensions = 1536;

        public int Id { get; set; }
        public int TenantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string MemoryType { get; set; } = string.Empty; // e.g., Conversation, Preference, Project, Person, Goal, Decision, Fact, Document, Task, Learning
        public string MemoryStatus { get; set; } = "Active";    // e.g., Active, Archived, Superseded, Incorrect
        public string Summary { get; set; } = string.Empty;
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public double ImportanceScore { get; set; }
        public int AccessCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastConfirmedAt { get; set; }

        // Relationships & Linkages
        public int? RelatedMemoryId { get; set; }
        public string? SourceEntityType { get; set; } // e.g., "Note", "Topic", "Course", "Task"
        public int? SourceEntityId { get; set; }      // ID of the source entity

        public MemoryItem? RelatedMemory { get; set; }
    }
}

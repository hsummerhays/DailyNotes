using System;
using System.ComponentModel.DataAnnotations;

namespace DailyNotes.Application.DTOs.Requests
{
    public class MemoryItemRequest
    {
        [Required]
        [MaxLength(50)]
        public string MemoryType { get; set; } = string.Empty; // Conversation, Preference, Project, Person, Goal, Decision, Fact, Document, Task, Learning
        [Required]
        [MaxLength(50)]
        public string MemoryStatus { get; set; } = "Active";    // Active, Archived, Superseded, Incorrect
        [Required]
        public string Summary { get; set; } = string.Empty;
        [Required]
        public float[] Embedding { get; set; } = Array.Empty<float>();
        [Range(0.0, 1.0)]
        public double ImportanceScore { get; set; }
        public DateTime? LastConfirmedAt { get; set; }

        public int? RelatedMemoryId { get; set; }
        [MaxLength(50)]
        public string? SourceEntityType { get; set; }
        public int? SourceEntityId { get; set; }
    }
}

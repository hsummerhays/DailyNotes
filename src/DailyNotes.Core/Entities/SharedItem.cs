using System;

namespace DailyNotes.Core.Entities
{
    public class SharedItem
    {
        public int Id { get; set; }
        public string ItemType { get; set; } = string.Empty; // 'project' | 'work_task' | 'work_note'
        public int ItemId { get; set; }
        public string SharedWithUserId { get; set; } = string.Empty;
        public string Permission { get; set; } = "read"; // 'read' | 'write'
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

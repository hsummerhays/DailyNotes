using System;
using System.Collections.Generic;

namespace DailyNotes.Core.Entities
{
    public class WebhookSubscription
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Url { get; set; } = string.Empty;
        public List<string> Events { get; set; } = new(); // ['note.created', 'task.completed']
        public string Secret { get; set; } = string.Empty; // HMAC signing secret
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

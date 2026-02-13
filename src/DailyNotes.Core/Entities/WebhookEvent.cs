using System;
using System.Text.Json;

namespace DailyNotes.Core.Entities
{
    public class WebhookEvent
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty; // 'meeting.ended' | 'message.created'
        public JsonDocument Payload { get; set; } = JsonDocument.Parse("{}");
        public bool Processed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

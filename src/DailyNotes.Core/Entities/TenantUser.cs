using System;
using System.Text.Json;
using DailyNotes.Core.Interfaces;

namespace DailyNotes.Core.Entities
{
    public class TenantUser : IHasTenantUser
    {
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Links to ASP.NET Identity User Id
        public string UserId { get; set; } = string.Empty;

        // 'owner' | 'member' | 'teacher' | 'student' | 'parent'
        public string Role { get; set; } = "member";

        // JSONB preferences
        public JsonDocument Preferences { get; set; } = JsonDocument.Parse("{}");

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

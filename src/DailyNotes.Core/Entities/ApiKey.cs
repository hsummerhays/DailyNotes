using System;
using System.Collections.Generic;
using DailyNotes.Core.Interfaces;

namespace DailyNotes.Core.Entities
{
    public class ApiKey : IHasTenantUser
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string KeyHash { get; set; } = string.Empty;
        public List<string> Scopes { get; set; } = new(); // ['read:notes', 'write:tasks', 'read:topics']
        public bool IsActive { get; set; } = true;
        public DateTime? LastUsedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

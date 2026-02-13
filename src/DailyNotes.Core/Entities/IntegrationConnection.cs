using System;

namespace DailyNotes.Core.Entities
{
    public class IntegrationConnection
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Provider { get; set; } = string.Empty; // 'jira' | 'salesforce' | 'slack' | 'teams' | 'zoom'
        public string? BaseUrl { get; set; }
        public string? EncryptedCredentials { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

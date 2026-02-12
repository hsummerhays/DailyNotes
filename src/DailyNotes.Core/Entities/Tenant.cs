using System;
using System.Collections.Generic;

namespace DailyNotes.Core.Entities
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Plan { get; set; } = "free"; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<TenantUser> Users { get; set; } = new List<TenantUser>();
    }
}

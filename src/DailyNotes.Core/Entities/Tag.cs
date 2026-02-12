using System;

namespace DailyNotes.Core.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#808080"; // Hex color code
    }
}

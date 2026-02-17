using System;
using DailyNotes.Core.Interfaces;

namespace DailyNotes.Core.Entities
{
    public class Tag : IHasTenant
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#808080"; // Hex color code
    }
}

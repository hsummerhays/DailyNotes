using System;
using DailyNotes.Core.Interfaces;

namespace DailyNotes.Core.Entities
{
    public class PayPeriod : IHasTenantUser
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public int Holidays { get; set; } = 0;
        public int PtoReported { get; set; } = 0;
        public string? PtoDaysOfMonth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

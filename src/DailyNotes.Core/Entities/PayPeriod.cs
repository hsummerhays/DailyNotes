using System;

namespace DailyNotes.Core.Entities
{
    public class PayPeriod
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public DateOnly PeriodEndDate { get; set; }
    }
}

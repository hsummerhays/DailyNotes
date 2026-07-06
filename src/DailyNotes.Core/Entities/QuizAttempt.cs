using System;
using DailyNotes.Core.Interfaces;

namespace DailyNotes.Core.Entities
{
    public class QuizAttempt : IHasTenant
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int QuizId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}

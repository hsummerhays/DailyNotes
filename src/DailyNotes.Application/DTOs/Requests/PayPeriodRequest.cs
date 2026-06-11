using System.ComponentModel.DataAnnotations;

namespace DailyNotes.Application.DTOs.Requests
{
    public class PayPeriodRequest
    {
        [Required]
        public DateOnly PeriodStartDate { get; set; }
        [Required]
        public DateOnly PeriodEndDate { get; set; }
        [Range(0, 30)]
        public int Holidays { get; set; } = 0;
        [Range(0, 200)]
        public int PtoReported { get; set; } = 0;
        [MaxLength(500)]
        public string? PtoDaysOfMonth { get; set; }
    }
}

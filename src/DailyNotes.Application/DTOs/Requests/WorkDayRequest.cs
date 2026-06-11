using System.ComponentModel.DataAnnotations;

namespace DailyNotes.Application.DTOs.Requests
{
    public class WorkDayRequest
    {
        [Required]
        public DateOnly WorkDate { get; set; }
        public TimeOnly? TimeIn1 { get; set; }
        public TimeOnly? TimeOut1 { get; set; }
        public TimeOnly? TimeIn2 { get; set; }
        public TimeOnly? TimeOut2 { get; set; }
        public TimeOnly? TimeIn3 { get; set; }
        public TimeOnly? TimeOut3 { get; set; }
        [Range(0, 1440)]
        public int BreakMinutes { get; set; } = 0;
        [MaxLength(2000)]
        public string? Comments { get; set; }
    }
}

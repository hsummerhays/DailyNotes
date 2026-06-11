using System.ComponentModel.DataAnnotations;

namespace DailyNotes.Application.DTOs.Requests
{
    public class AssignmentRequest
    {
        [Required]
        public int CourseId { get; set; }
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        [MaxLength(50)]
        public string Status { get; set; } = "pending";
        public decimal? Grade { get; set; }
        public decimal? MaxGrade { get; set; } = 100;
        public decimal? Weight { get; set; }
        public int? TopicId { get; set; }
    }
}

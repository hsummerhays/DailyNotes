using System.ComponentModel.DataAnnotations;

namespace DailyNotes.Application.DTOs.Requests
{
    public class CourseRequest
    {
        [Required]
        [MaxLength(100)]
        public string Semester { get; set; } = string.Empty;
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(255)]
        public string? Instructor { get; set; }
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;
        [Range(0, 20)]
        public int Credits { get; set; } = 3;
        public decimal? TargetGrade { get; set; }
        public decimal? CurrentGrade { get; set; }
        [MaxLength(100)]
        public string? ExternalSource { get; set; }
        [MaxLength(255)]
        public string? ExternalId { get; set; }
        [MaxLength(2000)]
        public string? ExternalUrl { get; set; }
        [Range(0, 100)]
        public int ProgressPercent { get; set; } = 0;
        public int? TopicId { get; set; }
        public bool IsPinned { get; set; } = false;
    }
}

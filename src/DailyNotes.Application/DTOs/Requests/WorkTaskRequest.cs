using System.ComponentModel.DataAnnotations;

namespace DailyNotes.Application.DTOs.Requests
{
    public class WorkTaskRequest
    {
        [Required]
        [MaxLength(500)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Status { get; set; } = "pending";
        [MaxLength(2000)]
        public string? Comments { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? DueDate { get; set; }
        public int? ProjectId { get; set; }
        public int? ParentTaskId { get; set; }
        [MaxLength(100)]
        public string? ExternalSource { get; set; }
        [MaxLength(255)]
        public string? ExternalId { get; set; }
        public bool IsPinned { get; set; } = false;
        [MaxLength(50)]
        public string Visibility { get; set; } = "private";
    }
}

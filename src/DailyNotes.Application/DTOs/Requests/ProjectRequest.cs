using System.ComponentModel.DataAnnotations;

namespace DailyNotes.Application.DTOs.Requests
{
    public class ProjectRequest
    {
        [Required]
        [MaxLength(500)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(255)]
        public string? Category { get; set; }
        public DateOnly? CreatedDate { get; set; }
        public DateOnly? CompletedDate { get; set; }
        [MaxLength(50)]
        public string Visibility { get; set; } = "private";
    }
}

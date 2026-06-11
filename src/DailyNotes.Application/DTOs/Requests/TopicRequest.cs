using System.ComponentModel.DataAnnotations;

namespace DailyNotes.Application.DTOs.Requests
{
    public class TopicRequest
    {
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string? Description { get; set; }
        public int? ParentTopicId { get; set; }
        [MaxLength(50)]
        public string Proficiency { get; set; } = "learning";
        [Range(1, 5)]
        public int SkillLevel { get; set; } = 1;
        public bool IsPinned { get; set; } = false;
        [MaxLength(50)]
        public string Visibility { get; set; } = "private";
    }
}

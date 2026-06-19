using System;
using System.ComponentModel.DataAnnotations;

namespace DailyNotes.Application.DTOs.Requests
{
    public class MemorySearchRequest
    {
        [Required]
        public float[] QueryEmbedding { get; set; } = Array.Empty<float>();
        [Range(0.0, 1.0)]
        public double MinImportanceScore { get; set; } = 0.0;
        [MaxLength(50)]
        public string? MemoryType { get; set; }
        [MaxLength(50)]
        public string MemoryStatus { get; set; } = "Active"; // Default to Active to exclude archived/superseded memories
        [Range(1, 50)]
        public int Limit { get; set; } = 5;
    }
}

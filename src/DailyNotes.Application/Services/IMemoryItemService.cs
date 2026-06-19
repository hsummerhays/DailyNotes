using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DailyNotes.Application.Services
{
    public interface IMemoryItemService
    {
        Task<IEnumerable<MemoryItem>> GetAllAsync();
        Task<MemoryItem?> GetByIdAsync(int id);
        Task<MemoryItem> CreateAsync(MemoryItemRequest request);
        Task<bool> UpdateAsync(int id, MemoryItemRequest request);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<MemoryItem>> SearchAsync(float[] queryEmbedding, double minImportanceScore = 0.0, double minConfidenceScore = 0.0, string? memoryType = null, string? memoryStatus = "Active", int limit = 5);
    }
}

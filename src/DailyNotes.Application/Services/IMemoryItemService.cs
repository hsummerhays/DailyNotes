using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DailyNotes.Application.Services
{
    public interface IMemoryItemService
    {
        Task<IEnumerable<MemoryItem>> GetAllAsync(CancellationToken ct = default);
        Task<MemoryItem?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<MemoryItem> CreateAsync(MemoryItemRequest request, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, MemoryItemRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<MemoryItem>> SearchAsync(float[] queryEmbedding, double minImportanceScore = 0.0, double minConfidenceScore = 0.0, string? memoryType = null, string? memoryStatus = "Active", int limit = 5, CancellationToken ct = default);
    }
}

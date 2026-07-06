using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IWorkTaskService
    {
        Task<IEnumerable<WorkTask>> GetAllAsync(string? status, int? projectId, CancellationToken ct = default);
        Task<IEnumerable<WorkTask>> GetOverdueAsync(CancellationToken ct = default);
        Task<WorkTask?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<WorkTask> CreateAsync(WorkTaskRequest request, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, WorkTaskRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}

using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IWorkTaskService
    {
        Task<IEnumerable<WorkTask>> GetAllAsync(string? status, int? projectId);
        Task<IEnumerable<WorkTask>> GetOverdueAsync();
        Task<WorkTask?> GetByIdAsync(int id);
        Task<WorkTask> CreateAsync(WorkTaskRequest request);
        Task<bool> UpdateAsync(int id, WorkTaskRequest request);
        Task<bool> DeleteAsync(int id);
    }
}

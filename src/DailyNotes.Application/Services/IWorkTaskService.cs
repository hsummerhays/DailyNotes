using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IWorkTaskService
    {
        Task<IEnumerable<WorkTask>> GetAllAsync(string? status, int? projectId);
        Task<IEnumerable<WorkTask>> GetOverdueAsync();
        Task<WorkTask?> GetByIdAsync(int id);
        Task<WorkTask> CreateAsync(WorkTask workTask);
        Task<bool> UpdateAsync(int id, WorkTask workTask);
        Task<bool> DeleteAsync(int id);
    }
}

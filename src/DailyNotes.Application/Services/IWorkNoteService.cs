using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IWorkNoteService
    {
        Task<IEnumerable<WorkNote>> GetAllAsync(DateOnly? date, int? taskId, int page, int pageSize);
        Task<WorkNote?> GetByIdAsync(int id);
        Task<WorkNote> CreateAsync(WorkNote workNote);
        Task<bool> UpdateAsync(int id, WorkNote workNote);
        Task<bool> DeleteAsync(int id);
    }
}

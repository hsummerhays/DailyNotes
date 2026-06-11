using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IWorkNoteService
    {
        Task<IEnumerable<WorkNote>> GetAllAsync(DateOnly? date, int? taskId, int page, int pageSize);
        Task<WorkNote?> GetByIdAsync(int id);
        Task<WorkNote> CreateAsync(WorkNoteRequest request);
        Task<bool> UpdateAsync(int id, WorkNoteRequest request);
        Task<bool> DeleteAsync(int id);
    }
}

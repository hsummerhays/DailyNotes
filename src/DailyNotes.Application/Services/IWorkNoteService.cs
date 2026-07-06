using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IWorkNoteService
    {
        Task<IEnumerable<WorkNote>> GetAllAsync(DateOnly? date, int? taskId, int page, int pageSize, CancellationToken ct = default);
        Task<WorkNote?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<WorkNote> CreateAsync(WorkNoteRequest request, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, WorkNoteRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}

using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IWorkDayService
    {
        Task<IEnumerable<WorkDay>> GetAllAsync(DateOnly? date, DateOnly? from, DateOnly? to, bool all, int page, int pageSize, CancellationToken ct = default);
        Task<WorkDay?> GetTodayAsync(CancellationToken ct = default);
        Task<WorkDay?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<WorkDay> CreateAsync(WorkDayRequest request, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, WorkDayRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}

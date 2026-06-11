using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IWorkDayService
    {
        Task<IEnumerable<WorkDay>> GetAllAsync(DateOnly? date, DateOnly? from, DateOnly? to, bool all, int page, int pageSize);
        Task<WorkDay?> GetTodayAsync();
        Task<WorkDay?> GetByIdAsync(int id);
        Task<WorkDay> CreateAsync(WorkDayRequest request);
        Task<bool> UpdateAsync(int id, WorkDayRequest request);
        Task<bool> DeleteAsync(int id);
    }
}

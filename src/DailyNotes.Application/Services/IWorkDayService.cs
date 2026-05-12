using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IWorkDayService
    {
        Task<IEnumerable<WorkDay>> GetAllAsync(DateOnly? date, DateOnly? from, DateOnly? to, bool all, int page, int pageSize);
        Task<WorkDay?> GetTodayAsync();
        Task<WorkDay?> GetByIdAsync(int id);
        Task<WorkDay> CreateAsync(WorkDay workDay);
        Task<bool> UpdateAsync(int id, WorkDay workDay);
        Task<bool> DeleteAsync(int id);
    }
}

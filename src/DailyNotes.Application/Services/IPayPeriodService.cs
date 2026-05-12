using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IPayPeriodService
    {
        Task<IEnumerable<PayPeriod>> GetAllAsync(DateOnly? date);
        Task<PayPeriod?> GetByIdAsync(int id);
        Task<IEnumerable<WorkDay>?> GetWorkDaysAsync(int id);
        Task<PayPeriod> CreateAsync(PayPeriod payPeriod);
        Task<bool> UpdateAsync(int id, PayPeriod payPeriod);
        Task<bool> DeleteAsync(int id);
    }
}

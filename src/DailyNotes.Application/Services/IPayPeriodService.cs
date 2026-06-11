using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IPayPeriodService
    {
        Task<IEnumerable<PayPeriod>> GetAllAsync(DateOnly? date);
        Task<PayPeriod?> GetByIdAsync(int id);
        Task<IEnumerable<WorkDay>?> GetWorkDaysAsync(int id);
        Task<PayPeriod> CreateAsync(PayPeriodRequest request);
        Task<bool> UpdateAsync(int id, PayPeriodRequest request);
        Task<bool> DeleteAsync(int id);
    }
}

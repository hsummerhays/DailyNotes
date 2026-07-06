using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IPayPeriodService
    {
        Task<IEnumerable<PayPeriod>> GetAllAsync(DateOnly? date, CancellationToken ct = default);
        Task<PayPeriod?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<WorkDay>?> GetWorkDaysAsync(int id, CancellationToken ct = default);
        Task<PayPeriod> CreateAsync(PayPeriodRequest request, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, PayPeriodRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}

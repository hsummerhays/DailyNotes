using DailyNotes.Application.Data;
using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class PayPeriodService : ApplicationServiceBase, IPayPeriodService
    {
        public PayPeriodService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock) : base(db, tc, clock) { }

        public async Task<IEnumerable<PayPeriod>> GetAllAsync(DateOnly? date)
        {
            var query = TenantScoped(_db.PayPeriods).AsQueryable();

            if (date.HasValue)
                query = query.Where(p => p.PeriodStartDate <= date.Value && p.PeriodEndDate >= date.Value);

            return await query.OrderByDescending(p => p.PeriodEndDate).ToListAsync();
        }

        public async Task<PayPeriod?> GetByIdAsync(int id)
            => await TenantScoped(_db.PayPeriods).FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<WorkDay>?> GetWorkDaysAsync(int id)
        {
            var period = await TenantScoped(_db.PayPeriods).FirstOrDefaultAsync(p => p.Id == id);
            if (period == null) return null;

            return await TenantScoped(_db.WorkDays)
                .Where(w => w.WorkDate >= period.PeriodStartDate && w.WorkDate <= period.PeriodEndDate)
                .OrderBy(w => w.WorkDate)
                .ToListAsync();
        }

        public async Task<PayPeriod> CreateAsync(PayPeriodRequest request)
        {
            var payPeriod = new PayPeriod
            {
                TenantId = _tc.TenantId,
                UserId = _tc.UserId,
                PeriodStartDate = request.PeriodStartDate,
                PeriodEndDate = request.PeriodEndDate,
                Holidays = request.Holidays,
                PtoReported = request.PtoReported,
                PtoDaysOfMonth = request.PtoDaysOfMonth,
                CreatedAt = _clock.GetUtcNow().UtcDateTime
            };

            _db.PayPeriods.Add(payPeriod);
            await _db.SaveChangesAsync();
            return payPeriod;
        }

        public async Task<bool> UpdateAsync(int id, PayPeriodRequest request)
        {
            var existing = await TenantScoped(_db.PayPeriods).FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null) return false;

            existing.PeriodStartDate = request.PeriodStartDate;
            existing.PeriodEndDate = request.PeriodEndDate;
            existing.Holidays = request.Holidays;
            existing.PtoReported = request.PtoReported;
            existing.PtoDaysOfMonth = request.PtoDaysOfMonth;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var period = await TenantScoped(_db.PayPeriods).FirstOrDefaultAsync(p => p.Id == id);
            if (period == null) return false;

            _db.PayPeriods.Remove(period);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

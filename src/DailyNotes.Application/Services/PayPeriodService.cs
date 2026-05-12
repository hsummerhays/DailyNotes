using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class PayPeriodService : ApplicationServiceBase, IPayPeriodService
    {
        public PayPeriodService(DailyNotesDbContext db, ITenantContext tc) : base(db, tc) { }

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

        public async Task<PayPeriod> CreateAsync(PayPeriod payPeriod)
        {
            payPeriod.TenantId = _tc.TenantId;
            payPeriod.UserId = _tc.UserId;
            payPeriod.CreatedAt = DateTime.UtcNow;

            _db.PayPeriods.Add(payPeriod);
            await _db.SaveChangesAsync();
            return payPeriod;
        }

        public async Task<bool> UpdateAsync(int id, PayPeriod payPeriod)
        {
            var existing = await TenantScoped(_db.PayPeriods).FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null) return false;

            existing.PeriodStartDate = payPeriod.PeriodStartDate;
            existing.PeriodEndDate = payPeriod.PeriodEndDate;
            existing.Holidays = payPeriod.Holidays;
            existing.PtoReported = payPeriod.PtoReported;
            existing.PtoDaysOfMonth = payPeriod.PtoDaysOfMonth;

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

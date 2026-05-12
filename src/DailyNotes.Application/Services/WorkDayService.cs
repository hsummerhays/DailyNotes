using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class WorkDayService : ApplicationServiceBase, IWorkDayService
    {
        public WorkDayService(DailyNotesDbContext db, ITenantContext tc) : base(db, tc) { }

        public async Task<IEnumerable<WorkDay>> GetAllAsync(
            DateOnly? date, DateOnly? from, DateOnly? to, bool all, int page, int pageSize)
        {
            var query = TenantScoped(_db.WorkDays).AsQueryable();

            if (!all)
            {
                if (date.HasValue)
                    query = query.Where(w => w.WorkDate == date.Value);
                else if (from.HasValue || to.HasValue)
                {
                    if (from.HasValue) query = query.Where(w => w.WorkDate >= from.Value);
                    if (to.HasValue) query = query.Where(w => w.WorkDate <= to.Value);
                }
                else
                {
                    var now = DateOnly.FromDateTime(DateTime.UtcNow);
                    var startDate = new DateOnly(now.Year, now.Month, 1);
                    var endDate = startDate.AddMonths(1).AddDays(-1);
                    query = query.Where(w => w.WorkDate >= startDate && w.WorkDate <= endDate);
                }
            }

            return await query
                .OrderByDescending(w => w.WorkDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<WorkDay?> GetTodayAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return await TenantScoped(_db.WorkDays)
                .Include(w => w.Notes)
                .FirstOrDefaultAsync(w => w.WorkDate == today);
        }

        public async Task<WorkDay?> GetByIdAsync(int id)
            => await TenantScoped(_db.WorkDays)
                .Include(w => w.Notes)
                .FirstOrDefaultAsync(w => w.Id == id);

        public async Task<WorkDay> CreateAsync(WorkDay workDay)
        {
            workDay.TenantId = _tc.TenantId;
            workDay.UserId = _tc.UserId;
            workDay.CreatedAt = DateTime.UtcNow;
            workDay.UpdatedAt = DateTime.UtcNow;

            _db.WorkDays.Add(workDay);
            await _db.SaveChangesAsync();
            return workDay;
        }

        public async Task<bool> UpdateAsync(int id, WorkDay workDay)
        {
            var existing = await TenantScoped(_db.WorkDays).FirstOrDefaultAsync(w => w.Id == id);
            if (existing == null) return false;

            existing.WorkDate = workDay.WorkDate;
            existing.TimeIn1 = workDay.TimeIn1;
            existing.TimeOut1 = workDay.TimeOut1;
            existing.TimeIn2 = workDay.TimeIn2;
            existing.TimeOut2 = workDay.TimeOut2;
            existing.TimeIn3 = workDay.TimeIn3;
            existing.TimeOut3 = workDay.TimeOut3;
            existing.BreakMinutes = workDay.BreakMinutes;
            existing.Comments = workDay.Comments;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var workDay = await TenantScoped(_db.WorkDays).FirstOrDefaultAsync(w => w.Id == id);
            if (workDay == null) return false;

            _db.WorkDays.Remove(workDay);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

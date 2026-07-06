using DailyNotes.Application.Data;
using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class WorkDayService : ApplicationServiceBase, IWorkDayService
    {
        public WorkDayService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock) : base(db, tc, clock) { }

        public async Task<IEnumerable<WorkDay>> GetAllAsync(
            DateOnly? date, DateOnly? from, DateOnly? to, bool all, int page, int pageSize, CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Min(pageSize, 100);
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
                    var now = DateOnly.FromDateTime(_clock.GetUtcNow().UtcDateTime);
                    var startDate = new DateOnly(now.Year, now.Month, 1);
                    var endDate = startDate.AddMonths(1).AddDays(-1);
                    query = query.Where(w => w.WorkDate >= startDate && w.WorkDate <= endDate);
                }
            }

            return await query
                .OrderByDescending(w => w.WorkDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<WorkDay?> GetTodayAsync(CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(_clock.GetUtcNow().UtcDateTime);
            return await TenantScoped(_db.WorkDays)
                .Include(w => w.Notes)
                .FirstOrDefaultAsync(w => w.WorkDate == today, ct);
        }

        public async Task<WorkDay?> GetByIdAsync(int id, CancellationToken ct = default)
            => await TenantScoped(_db.WorkDays)
                .Include(w => w.Notes)
                .FirstOrDefaultAsync(w => w.Id == id, ct);

        public async Task<WorkDay> CreateAsync(WorkDayRequest request, CancellationToken ct = default)
        {
            var now = _clock.GetUtcNow().UtcDateTime;
            var workDay = new WorkDay
            {
                TenantId = _tc.TenantId,
                UserId = _tc.UserId,
                WorkDate = request.WorkDate,
                TimeIn1 = request.TimeIn1,
                TimeOut1 = request.TimeOut1,
                TimeIn2 = request.TimeIn2,
                TimeOut2 = request.TimeOut2,
                TimeIn3 = request.TimeIn3,
                TimeOut3 = request.TimeOut3,
                BreakMinutes = request.BreakMinutes,
                Comments = request.Comments,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.WorkDays.Add(workDay);
            await _db.SaveChangesAsync(ct);
            return workDay;
        }

        public async Task<bool> UpdateAsync(int id, WorkDayRequest request, CancellationToken ct = default)
        {
            var existing = await TenantScoped(_db.WorkDays).FirstOrDefaultAsync(w => w.Id == id, ct);
            if (existing == null) return false;

            existing.WorkDate = request.WorkDate;
            existing.TimeIn1 = request.TimeIn1;
            existing.TimeOut1 = request.TimeOut1;
            existing.TimeIn2 = request.TimeIn2;
            existing.TimeOut2 = request.TimeOut2;
            existing.TimeIn3 = request.TimeIn3;
            existing.TimeOut3 = request.TimeOut3;
            existing.BreakMinutes = request.BreakMinutes;
            existing.Comments = request.Comments;
            existing.UpdatedAt = _clock.GetUtcNow().UtcDateTime;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var workDay = await TenantScoped(_db.WorkDays).FirstOrDefaultAsync(w => w.Id == id, ct);
            if (workDay == null) return false;

            _db.WorkDays.Remove(workDay);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}

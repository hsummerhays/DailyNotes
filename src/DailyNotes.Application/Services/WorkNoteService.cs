using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class WorkNoteService : ApplicationServiceBase, IWorkNoteService
    {
        public WorkNoteService(DailyNotesDbContext db, ITenantContext tc) : base(db, tc) { }

        public async Task<IEnumerable<WorkNote>> GetAllAsync(DateOnly? date, int? taskId, int page, int pageSize)
        {
            var query = TenantScoped(_db.WorkNotes).AsQueryable();

            if (date.HasValue) query = query.Where(n => n.NoteDate == date.Value);
            if (taskId.HasValue) query = query.Where(n => n.WorkTaskId == taskId.Value);

            return await query
                .OrderByDescending(n => n.NoteDate)
                .ThenByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<WorkNote?> GetByIdAsync(int id)
            => await TenantScoped(_db.WorkNotes).FirstOrDefaultAsync(n => n.Id == id);

        public async Task<WorkNote> CreateAsync(WorkNote workNote)
        {
            workNote.TenantId = _tc.TenantId;
            workNote.UserId = _tc.UserId;
            workNote.CreatedAt = DateTime.UtcNow;
            workNote.UpdatedAt = DateTime.UtcNow;

            // work_notes has a FK on NoteDate → work_days.WorkDate; ensure the row exists first
            using var transaction = await _db.Database.BeginTransactionAsync();

            var workDayExists = await _db.WorkDays.AnyAsync(d =>
                d.TenantId == _tc.TenantId &&
                d.UserId == _tc.UserId &&
                d.WorkDate == workNote.NoteDate);

            if (!workDayExists)
            {
                _db.WorkDays.Add(new WorkDay
                {
                    TenantId = _tc.TenantId,
                    UserId = _tc.UserId,
                    WorkDate = workNote.NoteDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }

            _db.WorkNotes.Add(workNote);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return workNote;
        }

        public async Task<bool> UpdateAsync(int id, WorkNote workNote)
        {
            var existing = await TenantScoped(_db.WorkNotes).FirstOrDefaultAsync(n => n.Id == id);
            if (existing == null) return false;

            existing.WorkTaskId = workNote.WorkTaskId;
            existing.NoteDate = workNote.NoteDate;
            existing.Content = workNote.Content;
            existing.TimeMinutes = workNote.TimeMinutes;
            existing.ExternalSource = workNote.ExternalSource;
            existing.ExternalId = workNote.ExternalId;
            existing.IsPinned = workNote.IsPinned;
            existing.Visibility = workNote.Visibility;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var note = await TenantScoped(_db.WorkNotes).FirstOrDefaultAsync(n => n.Id == id);
            if (note == null) return false;

            _db.WorkNotes.Remove(note);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

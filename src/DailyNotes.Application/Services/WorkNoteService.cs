using DailyNotes.Application.Data;
using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class WorkNoteService : ApplicationServiceBase, IWorkNoteService
    {
        public WorkNoteService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock) : base(db, tc, clock) { }

        public async Task<IEnumerable<WorkNote>> GetAllAsync(DateOnly? date, int? taskId, int page, int pageSize, CancellationToken ct = default)
        {
            pageSize = Math.Min(pageSize, 100);
            var query = TenantScoped(_db.WorkNotes).AsQueryable();

            if (date.HasValue) query = query.Where(n => n.NoteDate == date.Value);
            if (taskId.HasValue) query = query.Where(n => n.WorkTaskId == taskId.Value);

            return await query
                .OrderByDescending(n => n.NoteDate)
                .ThenByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<WorkNote?> GetByIdAsync(int id, CancellationToken ct = default)
            => await TenantScoped(_db.WorkNotes).FirstOrDefaultAsync(n => n.Id == id, ct);

        public async Task<WorkNote> CreateAsync(WorkNoteRequest request, CancellationToken ct = default)
        {
            var now = _clock.GetUtcNow().UtcDateTime;
            var workNote = new WorkNote
            {
                TenantId = _tc.TenantId,
                UserId = _tc.UserId,
                NoteDate = request.NoteDate,
                Content = request.Content,
                WorkTaskId = request.WorkTaskId,
                TimeMinutes = request.TimeMinutes,
                ExternalSource = request.ExternalSource,
                ExternalId = request.ExternalId,
                IsPinned = request.IsPinned,
                Visibility = request.Visibility,
                CreatedAt = now,
                UpdatedAt = now
            };

            // work_notes has a FK on NoteDate → work_days.WorkDate; ensure the row exists first
            using var transaction = await _db.Database.BeginTransactionAsync(ct);

            var workDayExists = await _db.WorkDays.AnyAsync(d =>
                d.TenantId == _tc.TenantId &&
                d.UserId == _tc.UserId &&
                d.WorkDate == workNote.NoteDate, ct);

            if (!workDayExists)
            {
                _db.WorkDays.Add(new WorkDay
                {
                    TenantId = _tc.TenantId,
                    UserId = _tc.UserId,
                    WorkDate = workNote.NoteDate,
                    CreatedAt = now,
                    UpdatedAt = now
                });
                await _db.SaveChangesAsync(ct);
            }

            _db.WorkNotes.Add(workNote);
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return workNote;
        }

        public async Task<bool> UpdateAsync(int id, WorkNoteRequest request, CancellationToken ct = default)
        {
            var existing = await TenantScoped(_db.WorkNotes).FirstOrDefaultAsync(n => n.Id == id, ct);
            if (existing == null) return false;

            existing.WorkTaskId = request.WorkTaskId;
            existing.NoteDate = request.NoteDate;
            existing.Content = request.Content;
            existing.TimeMinutes = request.TimeMinutes;
            existing.ExternalSource = request.ExternalSource;
            existing.ExternalId = request.ExternalId;
            existing.IsPinned = request.IsPinned;
            existing.Visibility = request.Visibility;
            existing.UpdatedAt = _clock.GetUtcNow().UtcDateTime;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var note = await TenantScoped(_db.WorkNotes).FirstOrDefaultAsync(n => n.Id == id, ct);
            if (note == null) return false;

            _db.WorkNotes.Remove(note);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}

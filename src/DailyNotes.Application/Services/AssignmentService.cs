using DailyNotes.Application.Data;
using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class AssignmentService : ApplicationServiceBase, IAssignmentService
    {
        public AssignmentService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock) : base(db, tc, clock) { }

        public async Task<IEnumerable<Assignment>> GetAllAsync(int? courseId, string? status, DateTime? dueDate, CancellationToken ct = default)
        {
            var query = TenantScoped(_db.Assignments).AsQueryable();

            if (courseId.HasValue) query = query.Where(a => a.CourseId == courseId.Value);
            if (!string.IsNullOrEmpty(status)) query = query.Where(a => a.Status == status);
            if (dueDate.HasValue) query = query.Where(a => a.DueDate.HasValue && a.DueDate.Value.Date == dueDate.Value.Date);

            return await query.OrderBy(a => a.DueDate).ToListAsync(ct);
        }

        public async Task<Assignment?> GetByIdAsync(int id, CancellationToken ct = default)
            => await TenantScoped(_db.Assignments).FirstOrDefaultAsync(a => a.Id == id, ct);

        public async Task<Assignment> CreateAsync(AssignmentRequest request, CancellationToken ct = default)
        {
            var now = _clock.GetUtcNow().UtcDateTime;
            var assignment = new Assignment
            {
                TenantId = _tc.TenantId,
                UserId = _tc.UserId,
                CourseId = request.CourseId,
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                Status = request.Status,
                Grade = request.Grade,
                MaxGrade = request.MaxGrade,
                Weight = request.Weight,
                TopicId = request.TopicId,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Assignments.Add(assignment);
            await _db.SaveChangesAsync(ct);
            return assignment;
        }

        public async Task<bool> UpdateAsync(int id, AssignmentRequest request, CancellationToken ct = default)
        {
            var existing = await TenantScoped(_db.Assignments).FirstOrDefaultAsync(a => a.Id == id, ct);
            if (existing == null) return false;

            existing.Title = request.Title;
            existing.Description = request.Description;
            existing.DueDate = request.DueDate;
            existing.Grade = request.Grade;
            existing.MaxGrade = request.MaxGrade;
            existing.Weight = request.Weight;
            existing.Status = request.Status;
            existing.TopicId = request.TopicId;
            existing.UpdatedAt = _clock.GetUtcNow().UtcDateTime;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var assignment = await TenantScoped(_db.Assignments).FirstOrDefaultAsync(a => a.Id == id, ct);
            if (assignment == null) return false;

            _db.Assignments.Remove(assignment);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}

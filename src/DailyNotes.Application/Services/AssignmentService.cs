using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class AssignmentService : ApplicationServiceBase, IAssignmentService
    {
        public AssignmentService(DailyNotesDbContext db, ITenantContext tc) : base(db, tc) { }

        public async Task<IEnumerable<Assignment>> GetAllAsync(int? courseId, string? status, DateTime? dueDate)
        {
            var query = TenantScoped(_db.Assignments).AsQueryable();

            if (courseId.HasValue) query = query.Where(a => a.CourseId == courseId.Value);
            if (!string.IsNullOrEmpty(status)) query = query.Where(a => a.Status == status);
            if (dueDate.HasValue) query = query.Where(a => a.DueDate.HasValue && a.DueDate.Value.Date == dueDate.Value.Date);

            return await query.OrderBy(a => a.DueDate).ToListAsync();
        }

        public async Task<Assignment?> GetByIdAsync(int id)
            => await TenantScoped(_db.Assignments).FirstOrDefaultAsync(a => a.Id == id);

        public async Task<Assignment> CreateAsync(Assignment assignment)
        {
            assignment.TenantId = _tc.TenantId;
            assignment.UserId = _tc.UserId;
            assignment.CreatedAt = DateTime.UtcNow;
            assignment.UpdatedAt = DateTime.UtcNow;

            _db.Assignments.Add(assignment);
            await _db.SaveChangesAsync();
            return assignment;
        }

        public async Task<bool> UpdateAsync(int id, Assignment assignment)
        {
            var existing = await TenantScoped(_db.Assignments).FirstOrDefaultAsync(a => a.Id == id);
            if (existing == null) return false;

            existing.Title = assignment.Title;
            existing.Description = assignment.Description;
            existing.DueDate = assignment.DueDate;
            existing.Grade = assignment.Grade;
            existing.MaxGrade = assignment.MaxGrade;
            existing.Weight = assignment.Weight;
            existing.Status = assignment.Status;
            existing.TopicId = assignment.TopicId;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var assignment = await TenantScoped(_db.Assignments).FirstOrDefaultAsync(a => a.Id == id);
            if (assignment == null) return false;

            _db.Assignments.Remove(assignment);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

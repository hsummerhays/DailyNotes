using DailyNotes.Application.Data;
using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class CourseService : ApplicationServiceBase, ICourseService
    {
        public CourseService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock) : base(db, tc, clock) { }

        public async Task<IEnumerable<Course>> GetAllAsync(string? semester, CancellationToken ct = default)
        {
            var query = TenantScoped(_db.Courses).AsQueryable();
            if (!string.IsNullOrEmpty(semester)) query = query.Where(c => c.Semester == semester);
            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync(ct);
        }

        public async Task<Course?> GetByIdAsync(int id, CancellationToken ct = default)
            => await TenantScoped(_db.Courses)
                .Include(c => c.Assignments)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

        public async Task<Course> CreateAsync(CourseRequest request, CancellationToken ct = default)
        {
            var now = _clock.GetUtcNow().UtcDateTime;
            var course = new Course
            {
                TenantId = _tc.TenantId,
                UserId = _tc.UserId,
                Semester = request.Semester,
                Name = request.Name,
                Instructor = request.Instructor,
                Description = request.Description,
                Credits = request.Credits,
                TargetGrade = request.TargetGrade,
                CurrentGrade = request.CurrentGrade,
                ExternalSource = request.ExternalSource,
                ExternalId = request.ExternalId,
                ExternalUrl = request.ExternalUrl,
                ProgressPercent = request.ProgressPercent,
                TopicId = request.TopicId,
                IsPinned = request.IsPinned,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Courses.Add(course);
            await _db.SaveChangesAsync(ct);
            return course;
        }

        public async Task<bool> UpdateAsync(int id, CourseRequest request, CancellationToken ct = default)
        {
            var existing = await TenantScoped(_db.Courses).FirstOrDefaultAsync(c => c.Id == id, ct);
            if (existing == null) return false;

            existing.Name = request.Name;
            existing.Instructor = request.Instructor;
            existing.Semester = request.Semester;
            existing.Credits = request.Credits;
            existing.CurrentGrade = request.CurrentGrade;
            existing.ExternalSource = request.ExternalSource;
            existing.ExternalId = request.ExternalId;
            existing.ExternalUrl = request.ExternalUrl;
            existing.ProgressPercent = request.ProgressPercent;
            existing.TopicId = request.TopicId;
            existing.IsPinned = request.IsPinned;
            existing.UpdatedAt = _clock.GetUtcNow().UtcDateTime;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var course = await TenantScoped(_db.Courses).FirstOrDefaultAsync(c => c.Id == id, ct);
            if (course == null) return false;

            _db.Courses.Remove(course);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}

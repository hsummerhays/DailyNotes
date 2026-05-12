using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class CourseService : ApplicationServiceBase, ICourseService
    {
        public CourseService(DailyNotesDbContext db, ITenantContext tc) : base(db, tc) { }

        public async Task<IEnumerable<Course>> GetAllAsync(string? semester)
        {
            var query = TenantScoped(_db.Courses).AsQueryable();
            if (!string.IsNullOrEmpty(semester)) query = query.Where(c => c.Semester == semester);
            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<Course?> GetByIdAsync(int id)
            => await TenantScoped(_db.Courses)
                .Include(c => c.Assignments)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Course> CreateAsync(Course course)
        {
            course.TenantId = _tc.TenantId;
            course.UserId = _tc.UserId;
            course.CreatedAt = DateTime.UtcNow;
            course.UpdatedAt = DateTime.UtcNow;

            _db.Courses.Add(course);
            await _db.SaveChangesAsync();
            return course;
        }

        public async Task<bool> UpdateAsync(int id, Course course)
        {
            var existing = await TenantScoped(_db.Courses).FirstOrDefaultAsync(c => c.Id == id);
            if (existing == null) return false;

            existing.Name = course.Name;
            existing.Instructor = course.Instructor;
            existing.Semester = course.Semester;
            existing.Credits = course.Credits;
            existing.CurrentGrade = course.CurrentGrade;
            existing.ExternalSource = course.ExternalSource;
            existing.ExternalId = course.ExternalId;
            existing.ExternalUrl = course.ExternalUrl;
            existing.ProgressPercent = course.ProgressPercent;
            existing.TopicId = course.TopicId;
            existing.IsPinned = course.IsPinned;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var course = await TenantScoped(_db.Courses).FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return false;

            _db.Courses.Remove(course);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

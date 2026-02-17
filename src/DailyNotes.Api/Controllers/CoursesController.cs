using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/courses")]
    [Authorize]
    public class CoursesController : ApiControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public CoursesController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>GET /api/courses?semester=Spring 2026</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetAll([FromQuery] string? semester)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var query = _context.Courses
                .Where(c => c.TenantId == tenantId && c.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(semester))
                query = query.Where(c => c.Semester == semester);

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetById(int id)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var course = await _context.Courses
                .Include(c => c.Assignments)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && c.UserId == userId);

            if (course == null)
                return NotFound();

            return course;
        }

        [HttpPost]
        public async Task<ActionResult<Course>> Create(Course course)
        {
            course.TenantId = CurrentTenantId;
            course.UserId = CurrentUserId;
            course.CreatedAt = DateTime.UtcNow;
            course.UpdatedAt = DateTime.UtcNow;

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Course course)
        {
            if (id != course.Id)
                return BadRequest(new { message = "ID mismatch." });

            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var existing = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && c.UserId == userId);
            if (existing == null)
                return NotFound();

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

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && c.UserId == userId);
            if (course == null)
                return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

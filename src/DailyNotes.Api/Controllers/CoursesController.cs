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

        /// <summary>Retrieves all courses, optionally filtered by semester.</summary>
        /// <param name="semester">Filter courses by a specific semester (e.g., 'Spring 2026').</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetAll([FromQuery] string? semester)
        {
            var query = TenantScoped(_context.Courses).AsQueryable();

            if (!string.IsNullOrEmpty(semester))
                query = query.Where(c => c.Semester == semester);

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        /// <summary>Retrieves a specific course by its ID, including its associated assignments.</summary>
        /// <param name="id">The unique ID of the course.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetById(int id)
        {
            var course = await TenantScoped(_context.Courses)
                .Include(c => c.Assignments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            return course;
        }

        /// <summary>Creates a new course.</summary>
        /// <param name="course">The course data to be created.</param>
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

        /// <summary>Updates an existing course record.</summary>
        /// <param name="id">The ID of the course to update.</param>
        /// <param name="course">The updated course data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Course course)
        {
            if (id != course.Id)
                return BadRequest(new { message = "ID mismatch." });

            var existing = await TenantScoped(_context.Courses)
                .FirstOrDefaultAsync(c => c.Id == id);
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

        /// <summary>Deletes a course record.</summary>
        /// <param name="id">The ID of the course to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await TenantScoped(_context.Courses)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (course == null)
                return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

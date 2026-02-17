using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/assignments")]
    [Authorize]
    public class AssignmentsController : ApiControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public AssignmentsController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>GET /api/assignments?courseId=&status=&dueDate=</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Assignment>>> GetAll(
            [FromQuery] int? courseId,
            [FromQuery] string? status,
            [FromQuery] DateTime? dueDate)
        {
            var query = TenantScoped(_context.Assignments).AsQueryable();

            if (courseId.HasValue)
                query = query.Where(a => a.CourseId == courseId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);

            if (dueDate.HasValue)
                query = query.Where(a => a.DueDate.HasValue && a.DueDate.Value.Date == dueDate.Value.Date);

            return await query.OrderBy(a => a.DueDate).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Assignment>> GetById(int id)
        {
            var assignment = await TenantScoped(_context.Assignments)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (assignment == null)
                return NotFound();

            return assignment;
        }

        [HttpPost]
        public async Task<ActionResult<Assignment>> Create(Assignment assignment)
        {
            assignment.TenantId = CurrentTenantId;
            assignment.UserId = CurrentUserId;
            assignment.CreatedAt = DateTime.UtcNow;
            assignment.UpdatedAt = DateTime.UtcNow;

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = assignment.Id }, assignment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Assignment assignment)
        {
            if (id != assignment.Id)
                return BadRequest(new { message = "ID mismatch." });

            var existing = await TenantScoped(_context.Assignments)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (existing == null)
                return NotFound();

            existing.Title = assignment.Title;
            existing.Description = assignment.Description;
            existing.DueDate = assignment.DueDate;
            existing.Grade = assignment.Grade;
            existing.MaxGrade = assignment.MaxGrade;
            existing.Weight = assignment.Weight;
            existing.Status = assignment.Status;
            existing.TopicId = assignment.TopicId;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var assignment = await TenantScoped(_context.Assignments)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (assignment == null)
                return NotFound();

            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

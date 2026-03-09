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

        /// <summary>Retrieves all assignments, optionally filtered by course, status, or due date.</summary>
        /// <param name="courseId">Filter assignments by course ID.</param>
        /// <param name="status">Filter assignments by status (e.g., 'pending', 'submitted').</param>
        /// <param name="dueDate">Filter assignments that are due on a specific date.</param>
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

        /// <summary>Retrieves a specific assignment by its ID.</summary>
        /// <param name="id">The unique ID of the assignment.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Assignment>> GetById(int id)
        {
            var assignment = await TenantScoped(_context.Assignments)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (assignment == null)
                return NotFound();

            return assignment;
        }

        /// <summary>Creates a new assignment.</summary>
        /// <param name="assignment">The assignment data to be created.</param>
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

        /// <summary>Updates an existing assignment record.</summary>
        /// <param name="id">The ID of the assignment to update.</param>
        /// <param name="assignment">The updated assignment data.</param>
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

        /// <summary>Deletes an assignment record.</summary>
        /// <param name="id">The ID of the assignment to delete.</param>
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

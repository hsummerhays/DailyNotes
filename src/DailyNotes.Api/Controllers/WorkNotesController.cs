using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/work-notes")]
    [Authorize]
    public class WorkNotesController : ApiControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public WorkNotesController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>Retrieves all work notes, optionally filtered by date or task ID, with pagination.</summary>
        /// <param name="date">Filter notes by a specific date.</param>
        /// <param name="taskId">Filter notes belonging to a specific task.</param>
        /// <param name="page">The page number for pagination (defaults to 1).</param>
        /// <param name="pageSize">The number of notes per page (defaults to 20).</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkNote>>> GetAll(
            [FromQuery] DateOnly? date,
            [FromQuery] int? taskId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = TenantScoped(_context.WorkNotes).AsQueryable();

            if (date.HasValue)
                query = query.Where(n => n.NoteDate == date.Value);

            if (taskId.HasValue)
                query = query.Where(n => n.WorkTaskId == taskId.Value);

            return await query
                .OrderByDescending(n => n.NoteDate)
                .ThenByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>Retrieves a specific work note by its unique ID.</summary>
        /// <param name="id">The unique ID of the note.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkNote>> GetById(int id)
        {
            var note = await TenantScoped(_context.WorkNotes)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (note == null)
                return NotFound();

            return note;
        }

        /// <summary>Creates a new work note. Also ensures the corresponding work day record exists.</summary>
        /// <param name="workNote">The work note data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<WorkNote>> Create(WorkNote workNote)
        {
            if (workNote.WorkTaskId == null)
                return BadRequest(new { message = "A linked task is required." });

            workNote.TenantId = CurrentTenantId;
            workNote.UserId = CurrentUserId;
            workNote.CreatedAt = DateTime.UtcNow;
            workNote.UpdatedAt = DateTime.UtcNow;

            // Ensure a WorkDay row exists for this date — work_notes has a FK on NoteDate → work_days.WorkDate
            var workDayExists = await _context.WorkDays.AnyAsync(d =>
                d.TenantId == workNote.TenantId &&
                d.UserId == workNote.UserId &&
                d.WorkDate == workNote.NoteDate);

            if (!workDayExists)
            {
                _context.WorkDays.Add(new WorkDay
                {
                    TenantId = workNote.TenantId,
                    UserId = workNote.UserId,
                    WorkDate = workNote.NoteDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            _context.WorkNotes.Add(workNote);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = workNote.Id }, workNote);
        }

        /// <summary>Updates an existing work note.</summary>
        /// <param name="id">The ID of the note to update.</param>
        /// <param name="workNote">The updated work note data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, WorkNote workNote)
        {
            if (id != workNote.Id)
                return BadRequest(new { message = "ID mismatch." });

            var existing = await TenantScoped(_context.WorkNotes)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (existing == null)
                return NotFound();

            existing.WorkTaskId = workNote.WorkTaskId;
            existing.NoteDate = workNote.NoteDate;
            existing.Content = workNote.Content;
            existing.TimeMinutes = workNote.TimeMinutes;
            existing.ExternalSource = workNote.ExternalSource;
            existing.ExternalId = workNote.ExternalId;
            existing.IsPinned = workNote.IsPinned;
            existing.Visibility = workNote.Visibility;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Deletes a work note.</summary>
        /// <param name="id">The ID of the note to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var note = await TenantScoped(_context.WorkNotes)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (note == null)
                return NotFound();

            _context.WorkNotes.Remove(note);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

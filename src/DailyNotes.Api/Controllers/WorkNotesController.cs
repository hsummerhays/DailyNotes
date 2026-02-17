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

        /// <summary>GET /api/work-notes?date=2025-01-15&taskId=1</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkNote>>> GetAll(
            [FromQuery] DateOnly? date,
            [FromQuery] int? taskId)
        {
            var query = TenantScoped(_context.WorkNotes).AsQueryable();

            if (date.HasValue)
                query = query.Where(n => n.NoteDate == date.Value);

            if (taskId.HasValue)
                query = query.Where(n => n.WorkTaskId == taskId.Value);

            return await query.OrderByDescending(n => n.NoteDate).ToListAsync();
        }

        /// <summary>GET /api/work-notes/{id}</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkNote>> GetById(int id)
        {
            var note = await TenantScoped(_context.WorkNotes)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (note == null)
                return NotFound();

            return note;
        }

        /// <summary>POST /api/work-notes</summary>
        [HttpPost]
        public async Task<ActionResult<WorkNote>> Create(WorkNote workNote)
        {
            workNote.TenantId = CurrentTenantId;
            workNote.UserId = CurrentUserId;
            workNote.CreatedAt = DateTime.UtcNow;
            workNote.UpdatedAt = DateTime.UtcNow;

            _context.WorkNotes.Add(workNote);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = workNote.Id }, workNote);
        }

        /// <summary>PUT /api/work-notes/{id}</summary>
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

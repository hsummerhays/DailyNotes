using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/work-days")]
    [Authorize]
    public class WorkDaysController : ControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public WorkDaysController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>GET /api/work-days?date=2025-01-15&from=&to=</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkDay>>> GetAll(
            [FromQuery] DateTime? date,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var query = _context.WorkDays.AsQueryable();

            if (date.HasValue)
                query = query.Where(w => w.WorkDate.Date == date.Value.Date);

            if (from.HasValue)
                query = query.Where(w => w.WorkDate >= from.Value);

            if (to.HasValue)
                query = query.Where(w => w.WorkDate <= to.Value);

            return await query.OrderByDescending(w => w.WorkDate).ToListAsync();
        }

        /// <summary>GET /api/work-days/today</summary>
        [HttpGet("today")]
        public async Task<ActionResult<WorkDay>> GetToday()
        {
            var today = DateTime.UtcNow.Date;
            var workDay = await _context.WorkDays
                .Include(w => w.Notes)
                .FirstOrDefaultAsync(w => w.WorkDate.Date == today);

            if (workDay == null)
                return NotFound(new { message = "No work day entry for today." });

            return workDay;
        }

        /// <summary>GET /api/work-days/{id}</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkDay>> GetById(int id)
        {
            var workDay = await _context.WorkDays
                .Include(w => w.Notes)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (workDay == null)
                return NotFound();

            return workDay;
        }

        /// <summary>POST /api/work-days</summary>
        [HttpPost]
        public async Task<ActionResult<WorkDay>> Create(WorkDay workDay)
        {
            workDay.CreatedAt = DateTime.UtcNow;
            workDay.UpdatedAt = DateTime.UtcNow;

            _context.WorkDays.Add(workDay);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = workDay.Id }, workDay);
        }

        /// <summary>PUT /api/work-days/{id}</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, WorkDay workDay)
        {
            if (id != workDay.Id)
                return BadRequest(new { message = "ID mismatch." });

            var existing = await _context.WorkDays.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.WorkDate = workDay.WorkDate;
            existing.TimeIn1 = workDay.TimeIn1;
            existing.TimeOut1 = workDay.TimeOut1;
            existing.TimeIn2 = workDay.TimeIn2;
            existing.TimeOut2 = workDay.TimeOut2;
            existing.TimeIn3 = workDay.TimeIn3;
            existing.TimeOut3 = workDay.TimeOut3;
            existing.BreakMinutes = workDay.BreakMinutes;
            existing.Comments = workDay.Comments;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>DELETE /api/work-days/{id}</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var workDay = await _context.WorkDays.FindAsync(id);
            if (workDay == null)
                return NotFound();

            _context.WorkDays.Remove(workDay);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

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
    public class WorkDaysController : ApiControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public WorkDaysController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>GET /api/work-days?date=2025-01-15&from=&to=</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkDay>>> GetAll(
            [FromQuery] DateOnly? date,
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            [FromQuery] bool all = false)
        {
            var query = TenantScoped(_context.WorkDays).AsQueryable();

            if (all)
            {
                // if 'all' is true, return everything, ignore other date filters
            }
            else if (date.HasValue)
            {
                query = query.Where(w => w.WorkDate == date.Value);
            }
            else if (from.HasValue || to.HasValue)
            {
                if (from.HasValue)
                    query = query.Where(w => w.WorkDate >= from.Value);
                if (to.HasValue)
                    query = query.Where(w => w.WorkDate <= to.Value);
            }
            else
            {
                // Default behavior: filter by current month
                var now = DateOnly.FromDateTime(DateTime.UtcNow);
                var startDate = new DateOnly(now.Year, now.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);
                query = query.Where(w => w.WorkDate >= startDate && w.WorkDate <= endDate);
            }

            return await query.OrderByDescending(w => w.WorkDate).ToListAsync();
        }

        /// <summary>GET /api/work-days/today</summary>
        [HttpGet("today")]
        public async Task<ActionResult<WorkDay>> GetToday()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var workDay = await TenantScoped(_context.WorkDays)
                .Include(w => w.Notes)
                .FirstOrDefaultAsync(w => w.WorkDate == today);

            if (workDay == null)
                return Ok(null);

            return workDay;
        }

        /// <summary>GET /api/work-days/{id}</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkDay>> GetById(int id)
        {
            var workDay = await TenantScoped(_context.WorkDays)
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
            workDay.TenantId = CurrentTenantId;
            workDay.UserId = CurrentUserId;
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

            var existing = await TenantScoped(_context.WorkDays)
                .FirstOrDefaultAsync(w => w.Id == id);
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
            var workDay = await TenantScoped(_context.WorkDays)
                .FirstOrDefaultAsync(w => w.Id == id);
            if (workDay == null)
                return NotFound();

            _context.WorkDays.Remove(workDay);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

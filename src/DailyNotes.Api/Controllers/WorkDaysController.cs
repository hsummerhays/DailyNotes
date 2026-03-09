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

        /// <summary>Retrieves all work days, optionally filtered by date or a date range.</summary>
        /// <param name="date">Filter by a single specific date.</param>
        /// <param name="from">The start date of a filter range.</param>
        /// <param name="to">The end date of a filter range.</param>
        /// <param name="all">If true, ignores date filters and returns all records.</param>
        /// <param name="page">The page number for pagination (defaults to 1).</param>
        /// <param name="pageSize">The number of records per page (defaults to 20).</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkDay>>> GetAll(
            [FromQuery] DateOnly? date,
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            [FromQuery] bool all = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
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

            return await query
                .OrderByDescending(w => w.WorkDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>Retrieves the work day record representing the current date.</summary>
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

        /// <summary>Retrieves a specific work day by its unique ID.</summary>
        /// <param name="id">The unique ID of the work day.</param>
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

        /// <summary>Creates a new work day record.</summary>
        /// <param name="workDay">The work day data to be created.</param>
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

        /// <summary>Updates an existing work day record.</summary>
        /// <param name="id">The ID of the work day to update.</param>
        /// <param name="workDay">The updated work day data.</param>
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

        /// <summary>Deletes a work day record.</summary>
        /// <param name="id">The ID of the record to delete.</param>
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

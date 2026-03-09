using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/pay-periods")]
    [Authorize]
    public class PayPeriodsController : ApiControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public PayPeriodsController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>Retrieves all pay periods, optionally filtered by a specific date.</summary>
        /// <param name="date">Filter to find the pay period that includes this date.</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PayPeriod>>> GetAll([FromQuery] DateOnly? date)
        {
            var query = TenantScoped(_context.PayPeriods).AsQueryable();

            if (date.HasValue)
                query = query.Where(p => p.PeriodStartDate <= date.Value && p.PeriodEndDate >= date.Value);

            return await query.OrderByDescending(p => p.PeriodEndDate).ToListAsync();
        }

        /// <summary>Retrieves a specific pay period by its ID.</summary>
        /// <param name="id">The unique ID of the pay period.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<PayPeriod>> GetById(int id)
        {
            var period = await TenantScoped(_context.PayPeriods)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (period == null)
                return NotFound();

            return period;
        }

        /// <summary>Retrieves all work day records that fall within a specific pay period's date range.</summary>
        /// <param name="id">The ID of the pay period.</param>
        [HttpGet("{id}/work-days")]
        public async Task<ActionResult<IEnumerable<WorkDay>>> GetPayPeriodWorkDays(int id)
        {
            var period = await TenantScoped(_context.PayPeriods)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (period == null)
                return NotFound();

            var workDays = await TenantScoped(_context.WorkDays)
                .Where(w => w.WorkDate >= period.PeriodStartDate
                         && w.WorkDate <= period.PeriodEndDate)
                .OrderBy(w => w.WorkDate)
                .ToListAsync();

            return workDays;
        }

        /// <summary>Creates a new pay period record.</summary>
        /// <param name="payPeriod">The pay period data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<PayPeriod>> Create(PayPeriod payPeriod)
        {
            payPeriod.TenantId = CurrentTenantId;
            payPeriod.UserId = CurrentUserId;
            payPeriod.CreatedAt = DateTime.UtcNow;

            _context.PayPeriods.Add(payPeriod);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = payPeriod.Id }, payPeriod);
        }

        /// <summary>Updates an existing pay period record.</summary>
        /// <param name="id">The ID of the pay period to update.</param>
        /// <param name="payPeriod">The updated pay period data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PayPeriod payPeriod)
        {
            if (id != payPeriod.Id)
                return BadRequest(new { message = "ID mismatch." });

            var existing = await TenantScoped(_context.PayPeriods)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null)
                return NotFound();

            existing.PeriodStartDate = payPeriod.PeriodStartDate;
            existing.PeriodEndDate = payPeriod.PeriodEndDate;
            existing.Holidays = payPeriod.Holidays;
            existing.PtoReported = payPeriod.PtoReported;
            existing.PtoDaysOfMonth = payPeriod.PtoDaysOfMonth;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Deletes a pay period record.</summary>
        /// <param name="id">The ID of the pay period to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var period = await TenantScoped(_context.PayPeriods)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (period == null)
                return NotFound();

            _context.PayPeriods.Remove(period);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

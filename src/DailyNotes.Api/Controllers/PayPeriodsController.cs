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

        /// <summary>GET /api/pay-periods?date=2025-01-15</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PayPeriod>>> GetAll([FromQuery] DateOnly? date)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var query = _context.PayPeriods
                .Where(p => p.TenantId == tenantId && p.UserId == userId)
                .AsQueryable();

            if (date.HasValue)
                query = query.Where(p => p.PeriodStartDate <= date.Value && p.PeriodEndDate >= date.Value);

            return await query.OrderByDescending(p => p.PeriodEndDate).ToListAsync();
        }

        /// <summary>GET /api/pay-periods/{id}</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PayPeriod>> GetById(int id)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var period = await _context.PayPeriods
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && p.UserId == userId);
            if (period == null)
                return NotFound();

            return period;
        }

        /// <summary>GET /api/pay-periods/{id}/work-days — all work days in a pay period</summary>
        [HttpGet("{id}/work-days")]
        public async Task<ActionResult<IEnumerable<WorkDay>>> GetPayPeriodWorkDays(int id)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var period = await _context.PayPeriods
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && p.UserId == userId);
            if (period == null)
                return NotFound();

            var workDays = await _context.WorkDays
                .Where(w => w.TenantId == tenantId && w.UserId == userId
                         && w.WorkDate >= period.PeriodStartDate
                         && w.WorkDate <= period.PeriodEndDate)
                .OrderBy(w => w.WorkDate)
                .ToListAsync();

            return workDays;
        }

        /// <summary>POST /api/pay-periods</summary>
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

        /// <summary>PUT /api/pay-periods/{id}</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PayPeriod payPeriod)
        {
            if (id != payPeriod.Id)
                return BadRequest(new { message = "ID mismatch." });

            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var existing = await _context.PayPeriods
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && p.UserId == userId);
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

        /// <summary>DELETE /api/pay-periods/{id}</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var period = await _context.PayPeriods
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && p.UserId == userId);
            if (period == null)
                return NotFound();

            _context.PayPeriods.Remove(period);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Application.Services;
using DailyNotes.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/pay-periods")]
    [Authorize]
    public class PayPeriodsController : ApiControllerBase
    {
        private readonly IPayPeriodService _service;

        public PayPeriodsController(IPayPeriodService service)
        {
            _service = service;
        }

        /// <summary>Retrieves all pay periods, optionally filtered by a specific date.</summary>
        /// <param name="date">Filter to find the pay period that includes this date.</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PayPeriod>>> GetAll([FromQuery] DateOnly? date)
            => Ok(await _service.GetAllAsync(date));

        /// <summary>Retrieves a specific pay period by its ID.</summary>
        /// <param name="id">The unique ID of the pay period.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<PayPeriod>> GetById(int id)
        {
            var period = await _service.GetByIdAsync(id);
            if (period == null) return NotFound();
            return period;
        }

        /// <summary>Retrieves all work day records that fall within a specific pay period's date range.</summary>
        /// <param name="id">The ID of the pay period.</param>
        [HttpGet("{id}/work-days")]
        public async Task<ActionResult<IEnumerable<WorkDay>>> GetPayPeriodWorkDays(int id)
        {
            var workDays = await _service.GetWorkDaysAsync(id);
            if (workDays == null) return NotFound();
            return Ok(workDays);
        }

        /// <summary>Creates a new pay period record.</summary>
        /// <param name="request">The pay period data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<PayPeriod>> Create(PayPeriodRequest request)
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates an existing pay period record.</summary>
        /// <param name="id">The ID of the pay period to update.</param>
        /// <param name="request">The updated pay period data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PayPeriodRequest request)
            => await _service.UpdateAsync(id, request) ? NoContent() : NotFound();

        /// <summary>Deletes a pay period record.</summary>
        /// <param name="id">The ID of the pay period to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}

using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Application.Services;
using DailyNotes.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/work-days")]
    [Authorize]
    public class WorkDaysController : ApiControllerBase
    {
        private readonly IWorkDayService _service;

        public WorkDaysController(IWorkDayService service)
        {
            _service = service;
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
            => Ok(await _service.GetAllAsync(date, from, to, all, page, pageSize));

        /// <summary>Retrieves the work day record representing the current date.</summary>
        [HttpGet("today")]
        public async Task<ActionResult<WorkDay>> GetToday()
            => Ok(await _service.GetTodayAsync());

        /// <summary>Retrieves a specific work day by its unique ID.</summary>
        /// <param name="id">The unique ID of the work day.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkDay>> GetById(int id)
        {
            var workDay = await _service.GetByIdAsync(id);
            if (workDay == null) return NotFound();
            return workDay;
        }

        /// <summary>Creates a new work day record.</summary>
        /// <param name="request">The work day data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<WorkDay>> Create(WorkDayRequest request)
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates an existing work day record.</summary>
        /// <param name="id">The ID of the work day to update.</param>
        /// <param name="request">The updated work day data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, WorkDayRequest request)
            => await _service.UpdateAsync(id, request) ? NoContent() : NotFound();

        /// <summary>Deletes a work day record.</summary>
        /// <param name="id">The ID of the record to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}

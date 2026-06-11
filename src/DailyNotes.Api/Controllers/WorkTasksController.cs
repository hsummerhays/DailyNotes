using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Application.Services;
using DailyNotes.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/work-tasks")]
    [Authorize]
    public class WorkTasksController : ApiControllerBase
    {
        private readonly IWorkTaskService _service;

        public WorkTasksController(IWorkTaskService service)
        {
            _service = service;
        }

        /// <summary>Retrieves all work tasks, optionally filtered by status or project.</summary>
        /// <param name="status">Filter tasks by status (e.g., 'pending', 'completed').</param>
        /// <param name="projectId">Filter tasks belonging to a specific project.</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkTask>>> GetAll(
            [FromQuery] string? status,
            [FromQuery] int? projectId)
            => Ok(await _service.GetAllAsync(status, projectId));

        /// <summary>Retrieves all overdue work tasks (due date in the past and not completed).</summary>
        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<WorkTask>>> GetOverdue()
            => Ok(await _service.GetOverdueAsync());

        /// <summary>Retrieves a specific work task by its ID.</summary>
        /// <param name="id">The unique ID of the task.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkTask>> GetById(int id)
        {
            var task = await _service.GetByIdAsync(id);
            if (task == null) return NotFound();
            return task;
        }

        /// <summary>Creates a new work task. A linked project is required.</summary>
        /// <param name="request">The task to be created.</param>
        [HttpPost]
        public async Task<ActionResult<WorkTask>> Create(WorkTaskRequest request)
        {
            if (request.ProjectId == null)
                return BadRequest(new { message = "A linked project is required." });

            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates an existing work task.</summary>
        /// <param name="id">The ID of the task to update.</param>
        /// <param name="request">The updated task content.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, WorkTaskRequest request)
            => await _service.UpdateAsync(id, request) ? NoContent() : NotFound();

        /// <summary>Deletes a work task.</summary>
        /// <param name="id">The ID of the task to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}

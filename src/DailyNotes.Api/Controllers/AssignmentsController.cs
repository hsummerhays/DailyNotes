using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Application.Services;
using DailyNotes.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/assignments")]
    [Authorize]
    public class AssignmentsController : ApiControllerBase
    {
        private readonly IAssignmentService _service;

        public AssignmentsController(IAssignmentService service)
        {
            _service = service;
        }

        /// <summary>Retrieves all assignments, optionally filtered by course, status, or due date.</summary>
        /// <param name="courseId">Filter assignments by course ID.</param>
        /// <param name="status">Filter assignments by status (e.g., 'pending', 'submitted').</param>
        /// <param name="dueDate">Filter assignments that are due on a specific date.</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Assignment>>> GetAll(
            [FromQuery] int? courseId,
            [FromQuery] string? status,
            [FromQuery] DateTime? dueDate)
            => Ok(await _service.GetAllAsync(courseId, status, dueDate));

        /// <summary>Retrieves a specific assignment by its ID.</summary>
        /// <param name="id">The unique ID of the assignment.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Assignment>> GetById(int id)
        {
            var assignment = await _service.GetByIdAsync(id);
            if (assignment == null) return NotFound();
            return assignment;
        }

        /// <summary>Creates a new assignment.</summary>
        /// <param name="request">The assignment data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<Assignment>> Create(AssignmentRequest request)
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates an existing assignment record.</summary>
        /// <param name="id">The ID of the assignment to update.</param>
        /// <param name="request">The updated assignment data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, AssignmentRequest request)
            => await _service.UpdateAsync(id, request) ? NoContent() : NotFound();

        /// <summary>Deletes an assignment record.</summary>
        /// <param name="id">The ID of the assignment to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}

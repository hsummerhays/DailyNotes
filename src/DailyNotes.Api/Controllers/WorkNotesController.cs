using DailyNotes.Application.Services;
using DailyNotes.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/work-notes")]
    [Authorize]
    public class WorkNotesController : ApiControllerBase
    {
        private readonly IWorkNoteService _service;

        public WorkNotesController(IWorkNoteService service)
        {
            _service = service;
        }

        /// <summary>Retrieves all work notes, optionally filtered by date or task ID, with pagination.</summary>
        /// <param name="date">Filter notes by a specific date.</param>
        /// <param name="taskId">Filter notes belonging to a specific task.</param>
        /// <param name="page">The page number for pagination (defaults to 1).</param>
        /// <param name="pageSize">The number of notes per page (defaults to 20).</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkNote>>> GetAll(
            [FromQuery] DateOnly? date,
            [FromQuery] int? taskId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
            => Ok(await _service.GetAllAsync(date, taskId, page, pageSize));

        /// <summary>Retrieves a specific work note by its unique ID.</summary>
        /// <param name="id">The unique ID of the note.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkNote>> GetById(int id)
        {
            var note = await _service.GetByIdAsync(id);
            if (note == null) return NotFound();
            return note;
        }

        /// <summary>Creates a new work note. Also ensures the corresponding work day record exists.</summary>
        /// <param name="workNote">The work note data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<WorkNote>> Create(WorkNote workNote)
        {
            if (workNote.WorkTaskId == null)
                return BadRequest(new { message = "A linked task is required." });

            var created = await _service.CreateAsync(workNote);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates an existing work note.</summary>
        /// <param name="id">The ID of the note to update.</param>
        /// <param name="workNote">The updated work note data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, WorkNote workNote)
        {
            if (id != workNote.Id) return BadRequest(new { message = "ID mismatch." });
            return await _service.UpdateAsync(id, workNote) ? NoContent() : NotFound();
        }

        /// <summary>Deletes a work note.</summary>
        /// <param name="id">The ID of the note to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}

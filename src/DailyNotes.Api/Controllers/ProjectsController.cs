using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Application.Services;
using DailyNotes.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Authorize]
    public class ProjectsController : ApiControllerBase
    {
        private readonly IProjectService _service;

        public ProjectsController(IProjectService service)
        {
            _service = service;
        }

        /// <summary>Retrieves all projects belonging to the current tenant.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetAll()
            => Ok(await _service.GetAllAsync(ct: HttpContext.RequestAborted));

        /// <summary>Retrieves a specific project by its ID.</summary>
        /// <param name="id">The unique ID of the project.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetById(int id)
        {
            var project = await _service.GetByIdAsync(id, ct: HttpContext.RequestAborted);
            if (project == null) return NotFound();
            return project;
        }

        /// <summary>Retrieves all work tasks associated with a specific project.</summary>
        /// <param name="id">The ID of the project.</param>
        [HttpGet("{id}/tasks")]
        public async Task<ActionResult<IEnumerable<WorkTask>>> GetProjectTasks(int id)
        {
            var tasks = await _service.GetProjectTasksAsync(id, ct: HttpContext.RequestAborted);
            if (tasks == null) return NotFound();
            return Ok(tasks);
        }

        /// <summary>Creates a new project.</summary>
        /// <param name="request">The project data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<Project>> Create(ProjectRequest request)
        {
            var created = await _service.CreateAsync(request, ct: HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates an existing project record.</summary>
        /// <param name="id">The ID of the project to update.</param>
        /// <param name="request">The updated project data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProjectRequest request)
            => await _service.UpdateAsync(id, request, ct: HttpContext.RequestAborted) ? NoContent() : NotFound();

        /// <summary>Deletes a project and its associated metadata (tasks remain but are unlinked if handled by DB).</summary>
        /// <param name="id">The ID of the project to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id, ct: HttpContext.RequestAborted) ? NoContent() : NotFound();
    }
}

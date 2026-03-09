using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Authorize]
    public class ProjectsController : ApiControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public ProjectsController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>Retrieves all projects belonging to the current tenant.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetAll()
        {
            return await TenantScoped(_context.Projects)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        /// <summary>Retrieves a specific project by its ID.</summary>
        /// <param name="id">The unique ID of the project.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetById(int id)
        {
            var project = await TenantScoped(_context.Projects)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                return NotFound();

            return project;
        }

        /// <summary>Retrieves all work tasks associated with a specific project.</summary>
        /// <param name="id">The ID of the project.</param>
        [HttpGet("{id}/tasks")]
        public async Task<ActionResult<IEnumerable<WorkTask>>> GetProjectTasks(int id)
        {
            var project = await TenantScoped(_context.Projects)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                return NotFound();

            var tasks = await TenantScoped(_context.WorkTasks)
                .Where(t => t.ProjectId == id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return tasks;
        }

        /// <summary>Creates a new project.</summary>
        /// <param name="project">The project data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<Project>> Create(Project project)
        {
            project.TenantId = CurrentTenantId;
            project.UserId = CurrentUserId;
            project.CreatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
        }

        /// <summary>Updates an existing project record.</summary>
        /// <param name="id">The ID of the project to update.</param>
        /// <param name="project">The updated project data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Project project)
        {
            if (id != project.Id)
                return BadRequest(new { message = "ID mismatch." });

            var existing = await TenantScoped(_context.Projects)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null)
                return NotFound();

            existing.Name = project.Name;
            existing.Category = project.Category;
            existing.Visibility = project.Visibility;
            existing.CreatedDate = project.CreatedDate;
            existing.CompletedDate = project.CompletedDate;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Deletes a project and its associated metadata (tasks remain but are unlinked if handled by DB).</summary>
        /// <param name="id">The ID of the project to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await TenantScoped(_context.Projects)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                return NotFound();

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

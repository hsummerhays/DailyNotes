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

        /// <summary>GET /api/projects</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetAll()
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            return await _context.Projects
                .Where(p => p.TenantId == tenantId && p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        /// <summary>GET /api/projects/{id}</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetById(int id)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && p.UserId == userId);
            if (project == null)
                return NotFound();

            return project;
        }

        /// <summary>GET /api/projects/{id}/tasks — all tasks for a project</summary>
        [HttpGet("{id}/tasks")]
        public async Task<ActionResult<IEnumerable<WorkTask>>> GetProjectTasks(int id)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && p.UserId == userId);
            if (project == null)
                return NotFound();

            var tasks = await _context.WorkTasks
                .Where(t => t.ProjectId == id && t.TenantId == tenantId && t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return tasks;
        }

        /// <summary>POST /api/projects</summary>
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

        /// <summary>PUT /api/projects/{id}</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Project project)
        {
            if (id != project.Id)
                return BadRequest(new { message = "ID mismatch." });

            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var existing = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && p.UserId == userId);
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

        /// <summary>DELETE /api/projects/{id}</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && p.UserId == userId);
            if (project == null)
                return NotFound();

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

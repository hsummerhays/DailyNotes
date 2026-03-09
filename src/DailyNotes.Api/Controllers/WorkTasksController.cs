using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/work-tasks")]
    [Authorize]
    public class WorkTasksController : ApiControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public WorkTasksController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>Retrieves all work tasks, optionally filtered by status or project.</summary>
        /// <param name="status">Filter tasks by status (e.g., 'pending', 'completed').</param>
        /// <param name="projectId">Filter tasks belonging to a specific project.</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkTask>>> GetAll(
            [FromQuery] string? status,
            [FromQuery] int? projectId)
        {
            var query = TenantScoped(_context.WorkTasks).AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId.Value);

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }


        /// <summary>Retrieves all overdue work tasks (due date in the past and not completed).</summary>
        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<WorkTask>>> GetOverdue()
        {
            var now = DateOnly.FromDateTime(DateTime.UtcNow);
            return await TenantScoped(_context.WorkTasks)
                .Where(t => t.DueDate.HasValue && t.DueDate < now && t.Status != "completed")
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        /// <summary>Retrieves a specific work task by its ID.</summary>
        /// <param name="id">The unique ID of the task.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkTask>> GetById(int id)
        {
            var task = await TenantScoped(_context.WorkTasks)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
                return NotFound();

            return task;
        }

        /// <summary>Creates a new work task. A linked project is required.</summary>
        /// <param name="workTask">The task to be created.</param>
        [HttpPost]
        public async Task<ActionResult<WorkTask>> Create(WorkTask workTask)
        {
            if (workTask.ProjectId == null)
                return BadRequest(new { message = "A linked project is required." });

            workTask.TenantId = CurrentTenantId;
            workTask.UserId = CurrentUserId;
            workTask.CreatedAt = DateTime.UtcNow;
            workTask.UpdatedAt = DateTime.UtcNow;

            _context.WorkTasks.Add(workTask);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = workTask.Id }, workTask);
        }

        /// <summary>Updates an existing work task.</summary>
        /// <param name="id">The ID of the task to update.</param>
        /// <param name="workTask">The updated task content.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, WorkTask workTask)
        {
            if (id != workTask.Id)
                return BadRequest(new { message = "ID mismatch." });

            var existing = await TenantScoped(_context.WorkTasks)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (existing == null)
                return NotFound();

            existing.Name = workTask.Name;
            existing.Status = workTask.Status;
            existing.StartDate = workTask.StartDate;
            existing.DueDate = workTask.DueDate;
            existing.ProjectId = workTask.ProjectId;
            existing.ParentTaskId = workTask.ParentTaskId;
            existing.ExternalSource = workTask.ExternalSource;
            existing.ExternalId = workTask.ExternalId;
            existing.IsPinned = workTask.IsPinned;
            existing.Visibility = workTask.Visibility;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Deletes a work task.</summary>
        /// <param name="id">The ID of the task to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await TenantScoped(_context.WorkTasks)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
                return NotFound();

            _context.WorkTasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

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

        /// <summary>GET /api/work-tasks?status=in_progress&projectId=1</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkTask>>> GetAll(
            [FromQuery] string? status,
            [FromQuery] int? projectId)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var query = _context.WorkTasks
                .Where(t => t.TenantId == tenantId && t.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId.Value);

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        /// <summary>GET /api/work-tasks/overdue</summary>
        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<WorkTask>>> GetOverdue()
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var now = DateOnly.FromDateTime(DateTime.UtcNow);
            return await _context.WorkTasks
                .Where(t => t.TenantId == tenantId && t.UserId == userId
                    && t.DueDate.HasValue && t.DueDate < now && t.Status != "completed")
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        /// <summary>GET /api/work-tasks/{id}</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkTask>> GetById(int id)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var task = await _context.WorkTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId && t.UserId == userId);
            if (task == null)
                return NotFound();

            return task;
        }

        /// <summary>POST /api/work-tasks</summary>
        [HttpPost]
        public async Task<ActionResult<WorkTask>> Create(WorkTask workTask)
        {
            workTask.TenantId = CurrentTenantId;
            workTask.UserId = CurrentUserId;
            workTask.CreatedAt = DateTime.UtcNow;
            workTask.UpdatedAt = DateTime.UtcNow;

            _context.WorkTasks.Add(workTask);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = workTask.Id }, workTask);
        }

        /// <summary>PUT /api/work-tasks/{id}</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, WorkTask workTask)
        {
            if (id != workTask.Id)
                return BadRequest(new { message = "ID mismatch." });

            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var existing = await _context.WorkTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId && t.UserId == userId);
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

        /// <summary>DELETE /api/work-tasks/{id}</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var task = await _context.WorkTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId && t.UserId == userId);
            if (task == null)
                return NotFound();

            _context.WorkTasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

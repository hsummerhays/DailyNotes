using DailyNotes.Application.Data;
using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class WorkTaskService : ApplicationServiceBase, IWorkTaskService
    {
        public WorkTaskService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock) : base(db, tc, clock) { }

        public async Task<IEnumerable<WorkTask>> GetAllAsync(string? status, int? projectId)
        {
            var query = TenantScoped(_db.WorkTasks).AsQueryable();

            if (!string.IsNullOrEmpty(status)) query = query.Where(t => t.Status == status);
            if (projectId.HasValue) query = query.Where(t => t.ProjectId == projectId.Value);

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<WorkTask>> GetOverdueAsync()
        {
            var now = DateOnly.FromDateTime(_clock.GetUtcNow().UtcDateTime);
            return await TenantScoped(_db.WorkTasks)
                .Where(t => t.DueDate.HasValue && t.DueDate < now && t.Status != "completed")
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<WorkTask?> GetByIdAsync(int id)
            => await TenantScoped(_db.WorkTasks).FirstOrDefaultAsync(t => t.Id == id);

        public async Task<WorkTask> CreateAsync(WorkTaskRequest request)
        {
            var now = _clock.GetUtcNow().UtcDateTime;
            var workTask = new WorkTask
            {
                TenantId = _tc.TenantId,
                UserId = _tc.UserId,
                Name = request.Name,
                Status = request.Status,
                Comments = request.Comments,
                StartDate = request.StartDate,
                DueDate = request.DueDate,
                ProjectId = request.ProjectId,
                ParentTaskId = request.ParentTaskId,
                ExternalSource = request.ExternalSource,
                ExternalId = request.ExternalId,
                IsPinned = request.IsPinned,
                Visibility = request.Visibility,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.WorkTasks.Add(workTask);
            await _db.SaveChangesAsync();
            return workTask;
        }

        public async Task<bool> UpdateAsync(int id, WorkTaskRequest request)
        {
            var existing = await TenantScoped(_db.WorkTasks).FirstOrDefaultAsync(t => t.Id == id);
            if (existing == null) return false;

            existing.Name = request.Name;
            existing.Status = request.Status;
            existing.StartDate = request.StartDate;
            existing.DueDate = request.DueDate;
            existing.ProjectId = request.ProjectId;
            existing.ParentTaskId = request.ParentTaskId;
            existing.ExternalSource = request.ExternalSource;
            existing.ExternalId = request.ExternalId;
            existing.IsPinned = request.IsPinned;
            existing.Visibility = request.Visibility;
            existing.Comments = request.Comments;
            existing.UpdatedAt = _clock.GetUtcNow().UtcDateTime;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await TenantScoped(_db.WorkTasks).FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return false;

            _db.WorkTasks.Remove(task);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

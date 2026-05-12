using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class WorkTaskService : ApplicationServiceBase, IWorkTaskService
    {
        public WorkTaskService(DailyNotesDbContext db, ITenantContext tc) : base(db, tc) { }

        public async Task<IEnumerable<WorkTask>> GetAllAsync(string? status, int? projectId)
        {
            var query = TenantScoped(_db.WorkTasks).AsQueryable();

            if (!string.IsNullOrEmpty(status)) query = query.Where(t => t.Status == status);
            if (projectId.HasValue) query = query.Where(t => t.ProjectId == projectId.Value);

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<WorkTask>> GetOverdueAsync()
        {
            var now = DateOnly.FromDateTime(DateTime.UtcNow);
            return await TenantScoped(_db.WorkTasks)
                .Where(t => t.DueDate.HasValue && t.DueDate < now && t.Status != "completed")
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<WorkTask?> GetByIdAsync(int id)
            => await TenantScoped(_db.WorkTasks).FirstOrDefaultAsync(t => t.Id == id);

        public async Task<WorkTask> CreateAsync(WorkTask workTask)
        {
            workTask.TenantId = _tc.TenantId;
            workTask.UserId = _tc.UserId;
            workTask.CreatedAt = DateTime.UtcNow;
            workTask.UpdatedAt = DateTime.UtcNow;

            _db.WorkTasks.Add(workTask);
            await _db.SaveChangesAsync();
            return workTask;
        }

        public async Task<bool> UpdateAsync(int id, WorkTask workTask)
        {
            var existing = await TenantScoped(_db.WorkTasks).FirstOrDefaultAsync(t => t.Id == id);
            if (existing == null) return false;

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
            existing.Comments = workTask.Comments;
            existing.UpdatedAt = DateTime.UtcNow;

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

using DailyNotes.Application.Data;
using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class ProjectService : ApplicationServiceBase, IProjectService
    {
        public ProjectService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock) : base(db, tc, clock) { }

        public async Task<IEnumerable<Project>> GetAllAsync()
            => await TenantScoped(_db.Projects).OrderByDescending(p => p.CreatedAt).ToListAsync();

        public async Task<Project?> GetByIdAsync(int id)
            => await TenantScoped(_db.Projects).FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<WorkTask>?> GetProjectTasksAsync(int id)
        {
            var project = await TenantScoped(_db.Projects).FirstOrDefaultAsync(p => p.Id == id);
            if (project == null) return null;

            return await TenantScoped(_db.WorkTasks)
                .Where(t => t.ProjectId == id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Project> CreateAsync(ProjectRequest request)
        {
            var now = _clock.GetUtcNow().UtcDateTime;
            var project = new Project
            {
                TenantId = _tc.TenantId,
                UserId = _tc.UserId,
                Name = request.Name,
                Category = request.Category,
                Visibility = request.Visibility,
                CreatedDate = request.CreatedDate,
                CompletedDate = request.CompletedDate,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
            return project;
        }

        public async Task<bool> UpdateAsync(int id, ProjectRequest request)
        {
            var existing = await TenantScoped(_db.Projects).FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null) return false;

            existing.Name = request.Name;
            existing.Category = request.Category;
            existing.Visibility = request.Visibility;
            existing.CreatedDate = request.CreatedDate;
            existing.CompletedDate = request.CompletedDate;
            existing.UpdatedAt = _clock.GetUtcNow().UtcDateTime;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var project = await TenantScoped(_db.Projects).FirstOrDefaultAsync(p => p.Id == id);
            if (project == null) return false;

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

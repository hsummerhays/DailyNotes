using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class ProjectService : ApplicationServiceBase, IProjectService
    {
        public ProjectService(DailyNotesDbContext db, ITenantContext tc) : base(db, tc) { }

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

        public async Task<Project> CreateAsync(Project project)
        {
            project.TenantId = _tc.TenantId;
            project.UserId = _tc.UserId;
            project.CreatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
            return project;
        }

        public async Task<bool> UpdateAsync(int id, Project project)
        {
            var existing = await TenantScoped(_db.Projects).FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null) return false;

            existing.Name = project.Name;
            existing.Category = project.Category;
            existing.Visibility = project.Visibility;
            existing.CreatedDate = project.CreatedDate;
            existing.CompletedDate = project.CompletedDate;
            existing.UpdatedAt = DateTime.UtcNow;

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

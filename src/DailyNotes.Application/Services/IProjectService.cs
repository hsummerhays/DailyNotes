using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllAsync();
        Task<Project?> GetByIdAsync(int id);
        Task<IEnumerable<WorkTask>?> GetProjectTasksAsync(int id);
        Task<Project> CreateAsync(Project project);
        Task<bool> UpdateAsync(int id, Project project);
        Task<bool> DeleteAsync(int id);
    }
}

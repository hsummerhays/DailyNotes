using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllAsync();
        Task<Project?> GetByIdAsync(int id);
        Task<IEnumerable<WorkTask>?> GetProjectTasksAsync(int id);
        Task<Project> CreateAsync(ProjectRequest request);
        Task<bool> UpdateAsync(int id, ProjectRequest request);
        Task<bool> DeleteAsync(int id);
    }
}

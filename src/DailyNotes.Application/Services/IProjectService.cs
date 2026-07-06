using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllAsync(CancellationToken ct = default);
        Task<Project?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<WorkTask>?> GetProjectTasksAsync(int id, CancellationToken ct = default);
        Task<Project> CreateAsync(ProjectRequest request, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, ProjectRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}

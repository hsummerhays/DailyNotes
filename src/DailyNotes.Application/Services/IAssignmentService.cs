using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IAssignmentService
    {
        Task<IEnumerable<Assignment>> GetAllAsync(int? courseId, string? status, DateTime? dueDate);
        Task<Assignment?> GetByIdAsync(int id);
        Task<Assignment> CreateAsync(Assignment assignment);
        Task<bool> UpdateAsync(int id, Assignment assignment);
        Task<bool> DeleteAsync(int id);
    }
}

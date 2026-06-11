using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IAssignmentService
    {
        Task<IEnumerable<Assignment>> GetAllAsync(int? courseId, string? status, DateTime? dueDate);
        Task<Assignment?> GetByIdAsync(int id);
        Task<Assignment> CreateAsync(AssignmentRequest request);
        Task<bool> UpdateAsync(int id, AssignmentRequest request);
        Task<bool> DeleteAsync(int id);
    }
}

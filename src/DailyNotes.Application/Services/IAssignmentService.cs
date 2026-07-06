using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IAssignmentService
    {
        Task<IEnumerable<Assignment>> GetAllAsync(int? courseId, string? status, DateTime? dueDate, CancellationToken ct = default);
        Task<Assignment?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Assignment> CreateAsync(AssignmentRequest request, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, AssignmentRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}

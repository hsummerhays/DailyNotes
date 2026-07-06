using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetAllAsync(string? semester, CancellationToken ct = default);
        Task<Course?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Course> CreateAsync(CourseRequest request, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, CourseRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}

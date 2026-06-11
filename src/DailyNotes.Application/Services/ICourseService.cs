using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetAllAsync(string? semester);
        Task<Course?> GetByIdAsync(int id);
        Task<Course> CreateAsync(CourseRequest request);
        Task<bool> UpdateAsync(int id, CourseRequest request);
        Task<bool> DeleteAsync(int id);
    }
}

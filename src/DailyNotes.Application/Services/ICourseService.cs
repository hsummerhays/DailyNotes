using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetAllAsync(string? semester);
        Task<Course?> GetByIdAsync(int id);
        Task<Course> CreateAsync(Course course);
        Task<bool> UpdateAsync(int id, Course course);
        Task<bool> DeleteAsync(int id);
    }
}

using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface ITopicService
    {
        Task<IEnumerable<Topic>> GetAllAsync(int? parentId, bool all);
        Task<Topic?> GetByIdAsync(int id);
        Task<IEnumerable<Topic>?> GetChildrenAsync(int id);
        Task<IEnumerable<TopicNote>?> GetNotesForTopicAsync(int id);
        Task<Topic> CreateAsync(TopicRequest request);
        Task<bool> UpdateAsync(int id, TopicRequest request);
        Task<bool> DeleteAsync(int id);
    }
}

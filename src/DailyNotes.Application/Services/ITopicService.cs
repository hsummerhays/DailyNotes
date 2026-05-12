using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface ITopicService
    {
        Task<IEnumerable<Topic>> GetAllAsync(int? parentId, bool all);
        Task<Topic?> GetByIdAsync(int id);
        Task<IEnumerable<Topic>?> GetChildrenAsync(int id);
        Task<IEnumerable<TopicNote>?> GetNotesForTopicAsync(int id);
        Task<Topic> CreateAsync(Topic topic);
        Task<bool> UpdateAsync(int id, Topic topic);
        Task<bool> DeleteAsync(int id);
    }
}

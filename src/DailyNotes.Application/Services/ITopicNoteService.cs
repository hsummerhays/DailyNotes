using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface ITopicNoteService
    {
        Task<IEnumerable<TopicNote>> GetAllAsync(int? topicId, int? tagId);
        Task<TopicNote?> GetByIdAsync(int id);
        Task<TopicNote> CreateAsync(TopicNote topicNote);
        Task<bool> UpdateAsync(int id, TopicNote topicNote);
        Task<bool> DeleteAsync(int id);
    }
}

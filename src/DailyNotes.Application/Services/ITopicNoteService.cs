using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface ITopicNoteService
    {
        Task<IEnumerable<TopicNote>> GetAllAsync(int? topicId, int? tagId);
        Task<TopicNote?> GetByIdAsync(int id);
        Task<TopicNote> CreateAsync(TopicNoteRequest request);
        Task<bool> UpdateAsync(int id, TopicNoteRequest request);
        Task<bool> DeleteAsync(int id);
    }
}

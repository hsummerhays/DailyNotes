using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface ITopicNoteService
    {
        Task<IEnumerable<TopicNote>> GetAllAsync(int? topicId, int? tagId, CancellationToken ct = default);
        Task<TopicNote?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<TopicNote> CreateAsync(TopicNoteRequest request, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, TopicNoteRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}

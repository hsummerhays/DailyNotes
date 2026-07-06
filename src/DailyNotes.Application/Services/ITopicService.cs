using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface ITopicService
    {
        Task<IEnumerable<Topic>> GetAllAsync(int? parentId, bool all, CancellationToken ct = default);
        Task<Topic?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<Topic>?> GetChildrenAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<TopicNote>?> GetNotesForTopicAsync(int id, CancellationToken ct = default);
        Task<Topic> CreateAsync(TopicRequest request, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, TopicRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}

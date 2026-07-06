using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface ITagService
    {
        Task<IEnumerable<Tag>> GetAllAsync(CancellationToken ct = default);
        Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Tag> CreateAsync(TagRequest request, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, TagRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<ItemTag?> TagItemAsync(int tagId, ItemTag itemTag, CancellationToken ct = default);
        Task<bool> UntagItemAsync(int tagId, string itemType, int itemId, CancellationToken ct = default);
        Task<IEnumerable<ItemTag>?> GetTaggedItemsAsync(int tagId, CancellationToken ct = default);
    }
}

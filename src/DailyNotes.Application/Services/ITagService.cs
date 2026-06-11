using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface ITagService
    {
        Task<IEnumerable<Tag>> GetAllAsync();
        Task<Tag?> GetByIdAsync(int id);
        Task<Tag> CreateAsync(TagRequest request);
        Task<bool> UpdateAsync(int id, TagRequest request);
        Task<bool> DeleteAsync(int id);
        Task<ItemTag?> TagItemAsync(int tagId, ItemTag itemTag);
        Task<bool> UntagItemAsync(int tagId, string itemType, int itemId);
        Task<IEnumerable<ItemTag>?> GetTaggedItemsAsync(int tagId);
    }
}

using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class TagService : ApplicationServiceBase, ITagService
    {
        public TagService(DailyNotesDbContext db, ITenantContext tc) : base(db, tc) { }

        public async Task<IEnumerable<Tag>> GetAllAsync()
            => await TenantOnlyScoped(_db.Tags).OrderBy(t => t.Name).ToListAsync();

        public async Task<Tag?> GetByIdAsync(int id)
            => await TenantOnlyScoped(_db.Tags).FirstOrDefaultAsync(t => t.Id == id);

        public async Task<Tag> CreateAsync(Tag tag)
        {
            tag.TenantId = _tc.TenantId;
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();
            return tag;
        }

        public async Task<bool> UpdateAsync(int id, Tag tag)
        {
            var existing = await TenantOnlyScoped(_db.Tags).FirstOrDefaultAsync(t => t.Id == id);
            if (existing == null) return false;

            existing.Name = tag.Name;
            existing.Color = tag.Color;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tag = await TenantOnlyScoped(_db.Tags).FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null) return false;

            _db.Tags.Remove(tag);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<ItemTag?> TagItemAsync(int tagId, ItemTag itemTag)
        {
            var tag = await TenantOnlyScoped(_db.Tags).FirstOrDefaultAsync(t => t.Id == tagId);
            if (tag == null) return null;

            itemTag.TagId = tagId;
            _db.ItemTags.Add(itemTag);
            await _db.SaveChangesAsync();
            return itemTag;
        }

        public async Task<bool> UntagItemAsync(int tagId, string itemType, int itemId)
        {
            var tag = await TenantOnlyScoped(_db.Tags).FirstOrDefaultAsync(t => t.Id == tagId);
            if (tag == null) return false;

            var itemTag = await _db.ItemTags
                .FirstOrDefaultAsync(it => it.TagId == tagId && it.ItemType == itemType && it.ItemId == itemId);
            if (itemTag == null) return false;

            _db.ItemTags.Remove(itemTag);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ItemTag>?> GetTaggedItemsAsync(int tagId)
        {
            var tag = await TenantOnlyScoped(_db.Tags).FirstOrDefaultAsync(t => t.Id == tagId);
            if (tag == null) return null;

            return await _db.ItemTags.Where(it => it.TagId == tagId).ToListAsync();
        }
    }
}

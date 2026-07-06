using DailyNotes.Application.Data;
using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class TagService : ApplicationServiceBase, ITagService
    {
        public TagService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock) : base(db, tc, clock) { }

        public async Task<IEnumerable<Tag>> GetAllAsync(CancellationToken ct = default)
            => await TenantOnlyScoped(_db.Tags).OrderBy(t => t.Name).ToListAsync(ct);

        public async Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default)
            => await TenantOnlyScoped(_db.Tags).FirstOrDefaultAsync(t => t.Id == id, ct);

        public async Task<Tag> CreateAsync(TagRequest request, CancellationToken ct = default)
        {
            var tag = new Tag
            {
                TenantId = _tc.TenantId,
                Name = request.Name,
                Color = request.Color
            };

            _db.Tags.Add(tag);
            await _db.SaveChangesAsync(ct);
            return tag;
        }

        public async Task<bool> UpdateAsync(int id, TagRequest request, CancellationToken ct = default)
        {
            var existing = await TenantOnlyScoped(_db.Tags).FirstOrDefaultAsync(t => t.Id == id, ct);
            if (existing == null) return false;

            existing.Name = request.Name;
            existing.Color = request.Color;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var tag = await TenantOnlyScoped(_db.Tags).FirstOrDefaultAsync(t => t.Id == id, ct);
            if (tag == null) return false;

            _db.Tags.Remove(tag);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<ItemTag?> TagItemAsync(int tagId, ItemTag itemTag, CancellationToken ct = default)
        {
            var tag = await TenantOnlyScoped(_db.Tags).FirstOrDefaultAsync(t => t.Id == tagId, ct);
            if (tag == null) return null;

            itemTag.TagId = tagId;
            _db.ItemTags.Add(itemTag);
            await _db.SaveChangesAsync(ct);
            return itemTag;
        }

        public async Task<bool> UntagItemAsync(int tagId, string itemType, int itemId, CancellationToken ct = default)
        {
            var tag = await TenantOnlyScoped(_db.Tags).FirstOrDefaultAsync(t => t.Id == tagId, ct);
            if (tag == null) return false;

            var itemTag = await _db.ItemTags
                .FirstOrDefaultAsync(it => it.TagId == tagId && it.ItemType == itemType && it.ItemId == itemId, ct);
            if (itemTag == null) return false;

            _db.ItemTags.Remove(itemTag);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<IEnumerable<ItemTag>?> GetTaggedItemsAsync(int tagId, CancellationToken ct = default)
        {
            var tag = await TenantOnlyScoped(_db.Tags).FirstOrDefaultAsync(t => t.Id == tagId, ct);
            if (tag == null) return null;

            return await _db.ItemTags.Where(it => it.TagId == tagId).ToListAsync(ct);
        }
    }
}

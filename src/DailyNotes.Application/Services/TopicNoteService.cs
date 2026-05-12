using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class TopicNoteService : ApplicationServiceBase, ITopicNoteService
    {
        public TopicNoteService(DailyNotesDbContext db, ITenantContext tc) : base(db, tc) { }

        public async Task<IEnumerable<TopicNote>> GetAllAsync(int? topicId, int? tagId)
        {
            var query = TenantScoped(_db.TopicNotes).AsQueryable();

            if (topicId.HasValue) query = query.Where(n => n.TopicId == topicId.Value);

            if (tagId.HasValue)
            {
                var taggedItemIds = _db.ItemTags
                    .Where(it => it.TagId == tagId.Value && it.ItemType == "topic_note")
                    .Select(it => it.ItemId);
                query = query.Where(n => taggedItemIds.Contains(n.Id));
            }

            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task<TopicNote?> GetByIdAsync(int id)
            => await TenantScoped(_db.TopicNotes).FirstOrDefaultAsync(n => n.Id == id);

        public async Task<TopicNote> CreateAsync(TopicNote topicNote)
        {
            topicNote.TenantId = _tc.TenantId;
            topicNote.UserId = _tc.UserId;
            topicNote.CreatedAt = DateTime.UtcNow;
            topicNote.UpdatedAt = DateTime.UtcNow;

            _db.TopicNotes.Add(topicNote);
            await _db.SaveChangesAsync();
            return topicNote;
        }

        public async Task<bool> UpdateAsync(int id, TopicNote topicNote)
        {
            var existing = await TenantScoped(_db.TopicNotes).FirstOrDefaultAsync(n => n.Id == id);
            if (existing == null) return false;

            existing.TopicId = topicNote.TopicId;
            existing.Title = topicNote.Title;
            existing.Content = topicNote.Content;
            existing.TimeMinutes = topicNote.TimeMinutes;
            existing.Visibility = topicNote.Visibility;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var note = await TenantScoped(_db.TopicNotes).FirstOrDefaultAsync(n => n.Id == id);
            if (note == null) return false;

            _db.TopicNotes.Remove(note);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

using DailyNotes.Application.Data;
using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class TopicNoteService : ApplicationServiceBase, ITopicNoteService
    {
        public TopicNoteService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock) : base(db, tc, clock) { }

        public async Task<IEnumerable<TopicNote>> GetAllAsync(int? topicId, int? tagId, CancellationToken ct = default)
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

            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync(ct);
        }

        public async Task<TopicNote?> GetByIdAsync(int id, CancellationToken ct = default)
            => await TenantScoped(_db.TopicNotes).FirstOrDefaultAsync(n => n.Id == id, ct);

        public async Task<TopicNote> CreateAsync(TopicNoteRequest request, CancellationToken ct = default)
        {
            var now = _clock.GetUtcNow().UtcDateTime;
            var topicNote = new TopicNote
            {
                TenantId = _tc.TenantId,
                UserId = _tc.UserId,
                TopicId = request.TopicId,
                Title = request.Title,
                Content = request.Content,
                TimeMinutes = request.TimeMinutes,
                Visibility = request.Visibility,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.TopicNotes.Add(topicNote);
            await _db.SaveChangesAsync(ct);
            return topicNote;
        }

        public async Task<bool> UpdateAsync(int id, TopicNoteRequest request, CancellationToken ct = default)
        {
            var existing = await TenantScoped(_db.TopicNotes).FirstOrDefaultAsync(n => n.Id == id, ct);
            if (existing == null) return false;

            existing.TopicId = request.TopicId;
            existing.Title = request.Title;
            existing.Content = request.Content;
            existing.TimeMinutes = request.TimeMinutes;
            existing.Visibility = request.Visibility;
            existing.UpdatedAt = _clock.GetUtcNow().UtcDateTime;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var note = await TenantScoped(_db.TopicNotes).FirstOrDefaultAsync(n => n.Id == id, ct);
            if (note == null) return false;

            _db.TopicNotes.Remove(note);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}

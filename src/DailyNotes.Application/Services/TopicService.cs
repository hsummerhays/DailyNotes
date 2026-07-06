using DailyNotes.Application.Data;
using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class TopicService : ApplicationServiceBase, ITopicService
    {
        public TopicService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock) : base(db, tc, clock) { }

        public async Task<IEnumerable<Topic>> GetAllAsync(int? parentId, bool all, CancellationToken ct = default)
        {
            var query = TenantScoped(_db.Topics).AsQueryable();

            if (!all)
            {
                query = parentId.HasValue
                    ? query.Where(t => t.ParentTopicId == parentId.Value)
                    : query.Where(t => t.ParentTopicId == null);
            }

            return await query.OrderBy(t => t.Title).ToListAsync(ct);
        }

        public async Task<Topic?> GetByIdAsync(int id, CancellationToken ct = default)
            => await TenantScoped(_db.Topics).FirstOrDefaultAsync(t => t.Id == id, ct);

        public async Task<IEnumerable<Topic>?> GetChildrenAsync(int id, CancellationToken ct = default)
        {
            var parent = await TenantScoped(_db.Topics).FirstOrDefaultAsync(t => t.Id == id, ct);
            if (parent == null) return null;

            return await TenantScoped(_db.Topics)
                .Where(t => t.ParentTopicId == id)
                .OrderBy(t => t.Title)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<TopicNote>?> GetNotesForTopicAsync(int id, CancellationToken ct = default)
        {
            var topic = await TenantScoped(_db.Topics).FirstOrDefaultAsync(t => t.Id == id, ct);
            if (topic == null) return null;

            return await TenantScoped(_db.TopicNotes)
                .Where(n => n.TopicId == id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<Topic> CreateAsync(TopicRequest request, CancellationToken ct = default)
        {
            var now = _clock.GetUtcNow().UtcDateTime;
            var topic = new Topic
            {
                TenantId = _tc.TenantId,
                UserId = _tc.UserId,
                Title = request.Title,
                Description = request.Description,
                ParentTopicId = request.ParentTopicId,
                Proficiency = request.Proficiency,
                SkillLevel = request.SkillLevel,
                IsPinned = request.IsPinned,
                Visibility = request.Visibility,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Topics.Add(topic);
            await _db.SaveChangesAsync(ct);
            return topic;
        }

        public async Task<bool> UpdateAsync(int id, TopicRequest request, CancellationToken ct = default)
        {
            var existing = await TenantScoped(_db.Topics).FirstOrDefaultAsync(t => t.Id == id, ct);
            if (existing == null) return false;

            existing.Title = request.Title;
            existing.Description = request.Description;
            existing.ParentTopicId = request.ParentTopicId;
            existing.Proficiency = request.Proficiency;
            existing.SkillLevel = request.SkillLevel;
            existing.Visibility = request.Visibility;
            existing.IsPinned = request.IsPinned;
            existing.UpdatedAt = _clock.GetUtcNow().UtcDateTime;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var topic = await TenantScoped(_db.Topics).FirstOrDefaultAsync(t => t.Id == id, ct);
            if (topic == null) return false;

            _db.Topics.Remove(topic);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}

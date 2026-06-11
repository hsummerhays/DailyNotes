using DailyNotes.Application.Data;
using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class TopicService : ApplicationServiceBase, ITopicService
    {
        public TopicService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock) : base(db, tc, clock) { }

        public async Task<IEnumerable<Topic>> GetAllAsync(int? parentId, bool all)
        {
            var query = TenantScoped(_db.Topics).AsQueryable();

            if (!all)
            {
                query = parentId.HasValue
                    ? query.Where(t => t.ParentTopicId == parentId.Value)
                    : query.Where(t => t.ParentTopicId == null);
            }

            return await query.OrderBy(t => t.Title).ToListAsync();
        }

        public async Task<Topic?> GetByIdAsync(int id)
            => await TenantScoped(_db.Topics).FirstOrDefaultAsync(t => t.Id == id);

        public async Task<IEnumerable<Topic>?> GetChildrenAsync(int id)
        {
            var parent = await TenantScoped(_db.Topics).FirstOrDefaultAsync(t => t.Id == id);
            if (parent == null) return null;

            return await TenantScoped(_db.Topics)
                .Where(t => t.ParentTopicId == id)
                .OrderBy(t => t.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<TopicNote>?> GetNotesForTopicAsync(int id)
        {
            var topic = await TenantScoped(_db.Topics).FirstOrDefaultAsync(t => t.Id == id);
            if (topic == null) return null;

            return await TenantScoped(_db.TopicNotes)
                .Where(n => n.TopicId == id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Topic> CreateAsync(TopicRequest request)
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
            await _db.SaveChangesAsync();
            return topic;
        }

        public async Task<bool> UpdateAsync(int id, TopicRequest request)
        {
            var existing = await TenantScoped(_db.Topics).FirstOrDefaultAsync(t => t.Id == id);
            if (existing == null) return false;

            existing.Title = request.Title;
            existing.Description = request.Description;
            existing.ParentTopicId = request.ParentTopicId;
            existing.Proficiency = request.Proficiency;
            existing.SkillLevel = request.SkillLevel;
            existing.Visibility = request.Visibility;
            existing.IsPinned = request.IsPinned;
            existing.UpdatedAt = _clock.GetUtcNow().UtcDateTime;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var topic = await TenantScoped(_db.Topics).FirstOrDefaultAsync(t => t.Id == id);
            if (topic == null) return false;

            _db.Topics.Remove(topic);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

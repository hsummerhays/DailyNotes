using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class TopicService : ApplicationServiceBase, ITopicService
    {
        public TopicService(DailyNotesDbContext db, ITenantContext tc) : base(db, tc) { }

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

        public async Task<Topic> CreateAsync(Topic topic)
        {
            topic.TenantId = _tc.TenantId;
            topic.UserId = _tc.UserId;
            topic.CreatedAt = DateTime.UtcNow;
            topic.UpdatedAt = DateTime.UtcNow;

            _db.Topics.Add(topic);
            await _db.SaveChangesAsync();
            return topic;
        }

        public async Task<bool> UpdateAsync(int id, Topic topic)
        {
            var existing = await TenantScoped(_db.Topics).FirstOrDefaultAsync(t => t.Id == id);
            if (existing == null) return false;

            existing.Title = topic.Title;
            existing.Description = topic.Description;
            existing.ParentTopicId = topic.ParentTopicId;
            existing.Proficiency = topic.Proficiency;
            existing.SkillLevel = topic.SkillLevel;
            existing.Visibility = topic.Visibility;
            existing.IsPinned = topic.IsPinned;
            existing.UpdatedAt = DateTime.UtcNow;

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

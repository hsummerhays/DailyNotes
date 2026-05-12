using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class AttachmentService : ApplicationServiceBase, IAttachmentService
    {
        public AttachmentService(DailyNotesDbContext db, ITenantContext tc) : base(db, tc) { }

        public async Task<IEnumerable<Attachment>> GetAllAsync(string? itemType, int? itemId)
        {
            var query = TenantScoped(_db.Attachments).AsQueryable();

            if (!string.IsNullOrEmpty(itemType)) query = query.Where(a => a.ItemType == itemType);
            if (itemId.HasValue) query = query.Where(a => a.ItemId == itemId.Value);

            return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        }

        public async Task<Attachment?> GetByIdAsync(int id)
            => await TenantScoped(_db.Attachments).FirstOrDefaultAsync(a => a.Id == id);

        public async Task<Attachment> CreateAsync(Attachment attachment)
        {
            attachment.TenantId = _tc.TenantId;
            attachment.UserId = _tc.UserId;
            attachment.CreatedAt = DateTime.UtcNow;

            _db.Attachments.Add(attachment);
            await _db.SaveChangesAsync();
            return attachment;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var attachment = await TenantScoped(_db.Attachments).FirstOrDefaultAsync(a => a.Id == id);
            if (attachment == null) return false;

            _db.Attachments.Remove(attachment);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

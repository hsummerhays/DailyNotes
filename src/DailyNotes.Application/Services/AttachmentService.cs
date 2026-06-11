using DailyNotes.Application.Data;
using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class AttachmentService : ApplicationServiceBase, IAttachmentService
    {
        public AttachmentService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock) : base(db, tc, clock) { }

        public async Task<IEnumerable<Attachment>> GetAllAsync(string? itemType, int? itemId)
        {
            var query = TenantScoped(_db.Attachments).AsQueryable();

            if (!string.IsNullOrEmpty(itemType)) query = query.Where(a => a.ItemType == itemType);
            if (itemId.HasValue) query = query.Where(a => a.ItemId == itemId.Value);

            return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        }

        public async Task<Attachment?> GetByIdAsync(int id)
            => await TenantScoped(_db.Attachments).FirstOrDefaultAsync(a => a.Id == id);

        public async Task<Attachment> CreateAsync(AttachmentRequest request)
        {
            var attachment = new Attachment
            {
                TenantId = _tc.TenantId,
                UserId = _tc.UserId,
                ItemType = request.ItemType,
                ItemId = request.ItemId,
                FileName = request.FileName,
                ContentType = request.ContentType,
                StoragePath = request.StoragePath,
                FileSizeBytes = request.FileSizeBytes,
                Source = request.Source,
                ExternalUrl = request.ExternalUrl,
                OcrText = request.OcrText,
                Transcription = request.Transcription,
                DurationSeconds = request.DurationSeconds,
                CreatedAt = _clock.GetUtcNow().UtcDateTime
            };

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

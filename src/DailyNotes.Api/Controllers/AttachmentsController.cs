using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/attachments")]
    [Authorize]
    public class AttachmentsController : ApiControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public AttachmentsController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>GET /api/attachments?itemType=work_note&itemId=1</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Attachment>>> GetAll(
            [FromQuery] string? itemType,
            [FromQuery] int? itemId)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var query = _context.Attachments
                .Where(a => a.TenantId == tenantId && a.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(itemType))
                query = query.Where(a => a.ItemType == itemType);

            if (itemId.HasValue)
                query = query.Where(a => a.ItemId == itemId.Value);

            return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Attachment>> GetById(int id)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var attachment = await _context.Attachments
                .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId && a.UserId == userId);
            if (attachment == null)
                return NotFound();

            return attachment;
        }

        /// <summary>POST /api/attachments — metadata-only create (file upload handled by storage provider)</summary>
        [HttpPost]
        public async Task<ActionResult<Attachment>> Create(Attachment attachment)
        {
            attachment.TenantId = CurrentTenantId;
            attachment.UserId = CurrentUserId;
            attachment.CreatedAt = DateTime.UtcNow;

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = attachment.Id }, attachment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            var attachment = await _context.Attachments
                .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId && a.UserId == userId);
            if (attachment == null)
                return NotFound();

            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

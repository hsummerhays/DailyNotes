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

        /// <summary>Retrieves all attachments, optionally filtered by item type and item ID.</summary>
        /// <param name="itemType">The type of the item the attachment belongs to (e.g., 'work_note').</param>
        /// <param name="itemId">The ID of the item the attachment belongs to.</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Attachment>>> GetAll(
            [FromQuery] string? itemType,
            [FromQuery] int? itemId)
        {
            var query = TenantScoped(_context.Attachments).AsQueryable();

            if (!string.IsNullOrEmpty(itemType))
                query = query.Where(a => a.ItemType == itemType);

            if (itemId.HasValue)
                query = query.Where(a => a.ItemId == itemId.Value);

            return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        }

        /// <summary>Retrieves a specific attachment by its ID.</summary>
        /// <param name="id">The unique ID of the attachment.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Attachment>> GetById(int id)
        {
            var attachment = await TenantScoped(_context.Attachments)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (attachment == null)
                return NotFound();

            return attachment;
        }

        /// <summary>Creates a new attachment record (metadata only). File upload should be handled by a storage provider.</summary>
        /// <param name="attachment">The attachment data to be created.</param>
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

        /// <summary>Deletes an attachment record.</summary>
        /// <param name="id">The ID of the attachment to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var attachment = await TenantScoped(_context.Attachments)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (attachment == null)
                return NotFound();

            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

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
    public class AttachmentsController : ControllerBase
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
            var query = _context.Attachments.AsQueryable();

            if (!string.IsNullOrEmpty(itemType))
                query = query.Where(a => a.ItemType == itemType);

            if (itemId.HasValue)
                query = query.Where(a => a.ItemId == itemId.Value);

            return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Attachment>> GetById(int id)
        {
            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment == null)
                return NotFound();

            return attachment;
        }

        /// <summary>POST /api/attachments — metadata-only create (file upload handled by storage provider)</summary>
        [HttpPost]
        public async Task<ActionResult<Attachment>> Create(Attachment attachment)
        {
            attachment.CreatedAt = DateTime.UtcNow;

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = attachment.Id }, attachment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment == null)
                return NotFound();

            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

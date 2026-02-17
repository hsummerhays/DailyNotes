using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/tags")]
    [Authorize]
    public class TagsController : ApiControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public TagsController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>GET /api/tags</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetAll()
        {
            var tenantId = CurrentTenantId;
            return await _context.Tags
                .Where(t => t.TenantId == tenantId)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tag>> GetById(int id)
        {
            var tenantId = CurrentTenantId;
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
            if (tag == null)
                return NotFound();

            return tag;
        }

        [HttpPost]
        public async Task<ActionResult<Tag>> Create(Tag tag)
        {
            tag.TenantId = CurrentTenantId;
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Tag tag)
        {
            if (id != tag.Id)
                return BadRequest(new { message = "ID mismatch." });

            var tenantId = CurrentTenantId;
            var existing = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
            if (existing == null)
                return NotFound();

            existing.Name = tag.Name;
            existing.Color = tag.Color;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = CurrentTenantId;
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
            if (tag == null)
                return NotFound();

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>POST /api/tags/{tagId}/items — tag an item</summary>
        [HttpPost("{tagId}/items")]
        public async Task<IActionResult> TagItem(int tagId, [FromBody] ItemTag itemTag)
        {
            var tenantId = CurrentTenantId;
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == tagId && t.TenantId == tenantId);
            if (tag == null)
                return NotFound(new { message = "Tag not found." });

            itemTag.TagId = tagId;
            _context.ItemTags.Add(itemTag);
            await _context.SaveChangesAsync();

            return Ok(itemTag);
        }

        /// <summary>DELETE /api/tags/{tagId}/items/{itemType}/{itemId} — untag an item</summary>
        [HttpDelete("{tagId}/items/{itemType}/{itemId}")]
        public async Task<IActionResult> UntagItem(int tagId, string itemType, int itemId)
        {
            var itemTag = await _context.ItemTags
                .FirstOrDefaultAsync(it => it.TagId == tagId && it.ItemType == itemType && it.ItemId == itemId);

            if (itemTag == null)
                return NotFound();

            _context.ItemTags.Remove(itemTag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>GET /api/tags/{tagId}/items — all items with this tag</summary>
        [HttpGet("{tagId}/items")]
        public async Task<ActionResult<IEnumerable<ItemTag>>> GetTaggedItems(int tagId)
        {
            return await _context.ItemTags
                .Where(it => it.TagId == tagId)
                .ToListAsync();
        }
    }
}

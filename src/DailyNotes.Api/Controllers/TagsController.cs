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

        /// <summary>Retrieves all tags belonging to the current tenant.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetAll()
        {
            return await TenantOnlyScoped(_context.Tags)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        /// <summary>Retrieves a specific tag by its ID.</summary>
        /// <param name="id">The unique ID of the tag.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Tag>> GetById(int id)
        {
            var tag = await TenantOnlyScoped(_context.Tags)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null)
                return NotFound();

            return tag;
        }

        /// <summary>Creates a new tag.</summary>
        /// <param name="tag">The tag data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<Tag>> Create(Tag tag)
        {
            tag.TenantId = CurrentTenantId;
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag);
        }

        /// <summary>Updates an existing tag record.</summary>
        /// <param name="id">The ID of the tag to update.</param>
        /// <param name="tag">The updated tag data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Tag tag)
        {
            if (id != tag.Id)
                return BadRequest(new { message = "ID mismatch." });

            var existing = await TenantOnlyScoped(_context.Tags)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (existing == null)
                return NotFound();

            existing.Name = tag.Name;
            existing.Color = tag.Color;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Deletes a tag record.</summary>
        /// <param name="id">The ID of the tag to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await TenantOnlyScoped(_context.Tags)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null)
                return NotFound();

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Associates a tag with a specific item (e.g., a note or task).</summary>
        /// <param name="tagId">The ID of the tag.</param>
        /// <param name="itemTag">The relation data specifying the item to be tagged.</param>
        [HttpPost("{tagId}/items")]
        public async Task<IActionResult> TagItem(int tagId, [FromBody] ItemTag itemTag)
        {
            var tag = await TenantOnlyScoped(_context.Tags)
                .FirstOrDefaultAsync(t => t.Id == tagId);
            if (tag == null)
                return NotFound(new { message = "Tag not found." });

            itemTag.TagId = tagId;
            _context.ItemTags.Add(itemTag);
            await _context.SaveChangesAsync();

            return Ok(itemTag);
        }

        /// <summary>Removes a tag from a specific item.</summary>
        /// <param name="tagId">The ID of the tag.</param>
        /// <param name="itemType">The type of the item (e.g., 'work_note').</param>
        /// <param name="itemId">The ID of the item.</param>
        [HttpDelete("{tagId}/items/{itemType}/{itemId}")]
        public async Task<IActionResult> UntagItem(int tagId, string itemType, int itemId)
        {
            // Verify tag ownership
            var tag = await TenantOnlyScoped(_context.Tags)
                .FirstOrDefaultAsync(t => t.Id == tagId);
            if (tag == null)
                return NotFound(new { message = "Tag not found." });

            var itemTag = await _context.ItemTags
                .FirstOrDefaultAsync(it => it.TagId == tagId && it.ItemType == itemType && it.ItemId == itemId);

            if (itemTag == null)
                return NotFound();

            _context.ItemTags.Remove(itemTag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Retrieves all item associations for a specific tag.</summary>
        /// <param name="tagId">The ID of the tag.</param>
        [HttpGet("{tagId}/items")]
        public async Task<ActionResult<IEnumerable<ItemTag>>> GetTaggedItems(int tagId)
        {
            // Verify tag ownership
            var tag = await TenantOnlyScoped(_context.Tags)
                .FirstOrDefaultAsync(t => t.Id == tagId);
            if (tag == null)
                return NotFound(new { message = "Tag not found." });

            return await _context.ItemTags
                .Where(it => it.TagId == tagId)
                .ToListAsync();
        }
    }
}

using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/topic-notes")]
    [Authorize]
    public class TopicNotesController : ApiControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public TopicNotesController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>Retrieves all topic notes, optionally filtered by topic or tag.</summary>
        /// <param name="topicId">Filter notes by a specific topic.</param>
        /// <param name="tagId">Filter notes that have a specific tag.</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TopicNote>>> GetAll(
            [FromQuery] int? topicId,
            [FromQuery] int? tagId)
        {
            var query = TenantScoped(_context.TopicNotes).AsQueryable();

            if (topicId.HasValue)
                query = query.Where(n => n.TopicId == topicId.Value);

            if (tagId.HasValue)
            {
                var taggedItemIds = _context.ItemTags
                    .Where(it => it.TagId == tagId.Value && it.ItemType == "topic_note")
                    .Select(it => it.ItemId);

                query = query.Where(n => taggedItemIds.Contains(n.Id));
            }

            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        /// <summary>Retrieves a specific topic note by its ID.</summary>
        /// <param name="id">The unique ID of the topic note.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<TopicNote>> GetById(int id)
        {
            var note = await TenantScoped(_context.TopicNotes)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (note == null)
                return NotFound();

            return note;
        }

        /// <summary>Creates a new topic note.</summary>
        /// <param name="topicNote">The note to be created.</param>
        [HttpPost]
        public async Task<ActionResult<TopicNote>> Create(TopicNote topicNote)
        {
            topicNote.TenantId = CurrentTenantId;
            topicNote.UserId = CurrentUserId;
            topicNote.CreatedAt = DateTime.UtcNow;
            topicNote.UpdatedAt = DateTime.UtcNow;

            _context.TopicNotes.Add(topicNote);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = topicNote.Id }, topicNote);
        }

        /// <summary>Updates an existing topic note.</summary>
        /// <param name="id">The ID of the note to update.</param>
        /// <param name="topicNote">The updated note content.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TopicNote topicNote)
        {
            if (id != topicNote.Id)
                return BadRequest(new { message = "ID mismatch." });

            var existing = await TenantScoped(_context.TopicNotes)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (existing == null)
                return NotFound();

            existing.TopicId = topicNote.TopicId;
            existing.Title = topicNote.Title;
            existing.Content = topicNote.Content;
            existing.TimeMinutes = topicNote.TimeMinutes;
            existing.Visibility = topicNote.Visibility;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Deletes a topic note.</summary>
        /// <param name="id">The ID of the note to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var note = await TenantScoped(_context.TopicNotes)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (note == null)
                return NotFound();

            _context.TopicNotes.Remove(note);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

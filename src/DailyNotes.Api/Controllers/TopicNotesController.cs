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

        /// <summary>GET /api/topic-notes?topicId=&tagId=</summary>
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

        [HttpGet("{id}")]
        public async Task<ActionResult<TopicNote>> GetById(int id)
        {
            var note = await TenantScoped(_context.TopicNotes)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (note == null)
                return NotFound();

            return note;
        }

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

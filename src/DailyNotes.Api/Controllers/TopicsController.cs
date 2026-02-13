using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/topics")]
    [Authorize]
    public class TopicsController : ControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public TopicsController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>GET /api/topics?parentId= (hierarchical)</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Topic>>> GetAll([FromQuery] int? parentId)
        {
            var query = _context.Topics.AsQueryable();

            if (parentId.HasValue)
                query = query.Where(t => t.ParentTopicId == parentId.Value);
            else
                query = query.Where(t => t.ParentTopicId == null); // Root topics

            return await query.OrderBy(t => t.Title).ToListAsync();
        }

        /// <summary>GET /api/topics/{id}</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Topic>> GetById(int id)
        {
            var topic = await _context.Topics.FindAsync(id);
            if (topic == null)
                return NotFound();

            return topic;
        }

        /// <summary>GET /api/topics/{id}/children — subtopics</summary>
        [HttpGet("{id}/children")]
        public async Task<ActionResult<IEnumerable<Topic>>> GetChildren(int id)
        {
            return await _context.Topics
                .Where(t => t.ParentTopicId == id)
                .OrderBy(t => t.Title)
                .ToListAsync();
        }

        /// <summary>GET /api/topics/{id}/notes</summary>
        [HttpGet("{id}/notes")]
        public async Task<ActionResult<IEnumerable<TopicNote>>> GetTopicNotes(int id)
        {
            var topic = await _context.Topics.FindAsync(id);
            if (topic == null)
                return NotFound();

            return await _context.TopicNotes
                .Where(n => n.TopicId == id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Topic>> Create(Topic topic)
        {
            topic.CreatedAt = DateTime.UtcNow;
            topic.UpdatedAt = DateTime.UtcNow;

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = topic.Id }, topic);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Topic topic)
        {
            if (id != topic.Id)
                return BadRequest(new { message = "ID mismatch." });

            var existing = await _context.Topics.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Title = topic.Title;
            existing.Description = topic.Description;
            existing.ParentTopicId = topic.ParentTopicId;
            existing.Proficiency = topic.Proficiency;
            existing.SkillLevel = topic.SkillLevel;
            existing.Visibility = topic.Visibility;
            existing.IsPinned = topic.IsPinned;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var topic = await _context.Topics.FindAsync(id);
            if (topic == null)
                return NotFound();

            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

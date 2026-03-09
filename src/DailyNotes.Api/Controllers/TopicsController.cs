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
    public class TopicsController : ApiControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public TopicsController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>Retrieves all topics, optionally filtered by a parent topic ID for hierarchy navigation.</summary>
        /// <param name="parentId">The ID of the parent topic. If null, root topics are returned.</param>
        /// <param name="all">If true, returns all topics regardless of parent.</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Topic>>> GetAll([FromQuery] int? parentId, [FromQuery] bool all = false)
        {
            var query = TenantScoped(_context.Topics).AsQueryable();

            if (!all)
            {
                if (parentId.HasValue)
                    query = query.Where(t => t.ParentTopicId == parentId.Value);
                else
                    query = query.Where(t => t.ParentTopicId == null); // Root topics
            }

            return await query.OrderBy(t => t.Title).ToListAsync();
        }

        /// <summary>Retrieves a specific topic by its ID.</summary>
        /// <param name="id">The unique ID of the topic.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Topic>> GetById(int id)
        {
            var topic = await TenantScoped(_context.Topics)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (topic == null)
                return NotFound();

            return topic;
        }

        /// <summary>Retrieves all child topics (subtopics) for a given topic.</summary>
        /// <param name="id">The ID of the parent topic.</param>
        [HttpGet("{id}/children")]
        public async Task<ActionResult<IEnumerable<Topic>>> GetChildren(int id)
        {
            // Verify parent exists/access
            var parent = await TenantScoped(_context.Topics)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (parent == null)
                return NotFound();

            return await TenantScoped(_context.Topics)
                .Where(t => t.ParentTopicId == id)
                .OrderBy(t => t.Title)
                .ToListAsync();
        }

        /// <summary>Retrieves all notes associated with a specific topic.</summary>
        /// <param name="id">The ID of the topic.</param>
        [HttpGet("{id}/notes")]
        public async Task<ActionResult<IEnumerable<TopicNote>>> GetTopicNotes(int id)
        {
            var topic = await TenantScoped(_context.Topics)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (topic == null)
                return NotFound();

            // TopicNotes likely need scoping too if they are entities. 
            // Assuming for now they are children of topic and we have access if we have access to topic.
            // But better to check. TopicNote likely has TenantId?
            return await _context.TopicNotes
                .Where(n => n.TopicId == id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        /// <summary>Creates a new topic.</summary>
        /// <param name="topic">The topic data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<Topic>> Create(Topic topic)
        {
            topic.TenantId = CurrentTenantId;
            topic.UserId = CurrentUserId;
            topic.CreatedAt = DateTime.UtcNow;
            topic.UpdatedAt = DateTime.UtcNow;

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = topic.Id }, topic);
        }

        /// <summary>Updates an existing topic.</summary>
        /// <param name="id">The ID of the topic to update.</param>
        /// <param name="topic">The updated topic data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Topic topic)
        {
            if (id != topic.Id)
                return BadRequest(new { message = "ID mismatch." });

            var existing = await TenantScoped(_context.Topics)
                .FirstOrDefaultAsync(t => t.Id == id);
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

        /// <summary>Deletes a topic and its associated subtopics/notes (depending on cascade rules).</summary>
        /// <param name="id">The ID of the topic to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var topic = await TenantScoped(_context.Topics)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (topic == null)
                return NotFound();

            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

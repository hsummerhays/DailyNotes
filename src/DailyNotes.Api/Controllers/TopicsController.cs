using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Application.Services;
using DailyNotes.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/topics")]
    [Authorize]
    public class TopicsController : ApiControllerBase
    {
        private readonly ITopicService _service;

        public TopicsController(ITopicService service)
        {
            _service = service;
        }

        /// <summary>Retrieves all topics, optionally filtered by a parent topic ID for hierarchy navigation.</summary>
        /// <param name="parentId">The ID of the parent topic. If null, root topics are returned.</param>
        /// <param name="all">If true, returns all topics regardless of parent.</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Topic>>> GetAll(
            [FromQuery] int? parentId,
            [FromQuery] bool all = false)
            => Ok(await _service.GetAllAsync(parentId, all));

        /// <summary>Retrieves a specific topic by its ID.</summary>
        /// <param name="id">The unique ID of the topic.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Topic>> GetById(int id)
        {
            var topic = await _service.GetByIdAsync(id);
            if (topic == null) return NotFound();
            return topic;
        }

        /// <summary>Retrieves all child topics (subtopics) for a given topic.</summary>
        /// <param name="id">The ID of the parent topic.</param>
        [HttpGet("{id}/children")]
        public async Task<ActionResult<IEnumerable<Topic>>> GetChildren(int id)
        {
            var children = await _service.GetChildrenAsync(id);
            if (children == null) return NotFound();
            return Ok(children);
        }

        /// <summary>Retrieves all notes associated with a specific topic.</summary>
        /// <param name="id">The ID of the topic.</param>
        [HttpGet("{id}/notes")]
        public async Task<ActionResult<IEnumerable<TopicNote>>> GetTopicNotes(int id)
        {
            var notes = await _service.GetNotesForTopicAsync(id);
            if (notes == null) return NotFound();
            return Ok(notes);
        }

        /// <summary>Creates a new topic.</summary>
        /// <param name="request">The topic data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<Topic>> Create(TopicRequest request)
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates an existing topic.</summary>
        /// <param name="id">The ID of the topic to update.</param>
        /// <param name="request">The updated topic data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TopicRequest request)
            => await _service.UpdateAsync(id, request) ? NoContent() : NotFound();

        /// <summary>Deletes a topic and its associated subtopics/notes (depending on cascade rules).</summary>
        /// <param name="id">The ID of the topic to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}

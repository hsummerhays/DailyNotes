using DailyNotes.Application.Services;
using DailyNotes.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/topic-notes")]
    [Authorize]
    public class TopicNotesController : ApiControllerBase
    {
        private readonly ITopicNoteService _service;

        public TopicNotesController(ITopicNoteService service)
        {
            _service = service;
        }

        /// <summary>Retrieves all topic notes, optionally filtered by topic or tag.</summary>
        /// <param name="topicId">Filter notes by a specific topic.</param>
        /// <param name="tagId">Filter notes that have a specific tag.</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TopicNote>>> GetAll(
            [FromQuery] int? topicId,
            [FromQuery] int? tagId)
            => Ok(await _service.GetAllAsync(topicId, tagId));

        /// <summary>Retrieves a specific topic note by its ID.</summary>
        /// <param name="id">The unique ID of the topic note.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<TopicNote>> GetById(int id)
        {
            var note = await _service.GetByIdAsync(id);
            if (note == null) return NotFound();
            return note;
        }

        /// <summary>Creates a new topic note.</summary>
        /// <param name="topicNote">The note to be created.</param>
        [HttpPost]
        public async Task<ActionResult<TopicNote>> Create(TopicNote topicNote)
        {
            var created = await _service.CreateAsync(topicNote);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates an existing topic note.</summary>
        /// <param name="id">The ID of the note to update.</param>
        /// <param name="topicNote">The updated note content.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TopicNote topicNote)
        {
            if (id != topicNote.Id) return BadRequest(new { message = "ID mismatch." });
            return await _service.UpdateAsync(id, topicNote) ? NoContent() : NotFound();
        }

        /// <summary>Deletes a topic note.</summary>
        /// <param name="id">The ID of the note to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}

using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Application.Services;
using DailyNotes.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/tags")]
    [Authorize]
    public class TagsController : ApiControllerBase
    {
        private readonly ITagService _service;

        public TagsController(ITagService service)
        {
            _service = service;
        }

        /// <summary>Retrieves all tags belonging to the current tenant.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetAll()
            => Ok(await _service.GetAllAsync(ct: HttpContext.RequestAborted));

        /// <summary>Retrieves a specific tag by its ID.</summary>
        /// <param name="id">The unique ID of the tag.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Tag>> GetById(int id)
        {
            var tag = await _service.GetByIdAsync(id, ct: HttpContext.RequestAborted);
            if (tag == null) return NotFound();
            return tag;
        }

        /// <summary>Creates a new tag.</summary>
        /// <param name="request">The tag data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<Tag>> Create(TagRequest request)
        {
            var created = await _service.CreateAsync(request, ct: HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates an existing tag record.</summary>
        /// <param name="id">The ID of the tag to update.</param>
        /// <param name="request">The updated tag data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TagRequest request)
            => await _service.UpdateAsync(id, request, ct: HttpContext.RequestAborted) ? NoContent() : NotFound();

        /// <summary>Deletes a tag record.</summary>
        /// <param name="id">The ID of the tag to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id, ct: HttpContext.RequestAborted) ? NoContent() : NotFound();

        /// <summary>Associates a tag with a specific item (e.g., a note or task).</summary>
        /// <param name="tagId">The ID of the tag.</param>
        /// <param name="itemTag">The relation data specifying the item to be tagged.</param>
        [HttpPost("{tagId}/items")]
        public async Task<IActionResult> TagItem(int tagId, [FromBody] ItemTag itemTag)
        {
            var result = await _service.TagItemAsync(tagId, itemTag, ct: HttpContext.RequestAborted);
            if (result == null) return NotFound(new { message = "Tag not found." });
            return Ok(result);
        }

        /// <summary>Removes a tag from a specific item.</summary>
        /// <param name="tagId">The ID of the tag.</param>
        /// <param name="itemType">The type of the item (e.g., 'work_note').</param>
        /// <param name="itemId">The ID of the item.</param>
        [HttpDelete("{tagId}/items/{itemType}/{itemId}")]
        public async Task<IActionResult> UntagItem(int tagId, string itemType, int itemId)
            => await _service.UntagItemAsync(tagId, itemType, itemId, ct: HttpContext.RequestAborted) ? NoContent() : NotFound();

        /// <summary>Retrieves all item associations for a specific tag.</summary>
        /// <param name="tagId">The ID of the tag.</param>
        [HttpGet("{tagId}/items")]
        public async Task<ActionResult<IEnumerable<ItemTag>>> GetTaggedItems(int tagId)
        {
            var items = await _service.GetTaggedItemsAsync(tagId, ct: HttpContext.RequestAborted);
            if (items == null) return NotFound(new { message = "Tag not found." });
            return Ok(items);
        }
    }
}

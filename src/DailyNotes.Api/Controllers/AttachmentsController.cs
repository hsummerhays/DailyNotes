using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Application.Services;
using DailyNotes.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/attachments")]
    [Authorize]
    public class AttachmentsController : ApiControllerBase
    {
        private readonly IAttachmentService _service;

        public AttachmentsController(IAttachmentService service)
        {
            _service = service;
        }

        /// <summary>Retrieves all attachments, optionally filtered by item type and item ID.</summary>
        /// <param name="itemType">The type of the item the attachment belongs to (e.g., 'work_note').</param>
        /// <param name="itemId">The ID of the item the attachment belongs to.</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Attachment>>> GetAll(
            [FromQuery] string? itemType,
            [FromQuery] int? itemId)
            => Ok(await _service.GetAllAsync(itemType, itemId, ct: HttpContext.RequestAborted));

        /// <summary>Retrieves a specific attachment by its ID.</summary>
        /// <param name="id">The unique ID of the attachment.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Attachment>> GetById(int id)
        {
            var attachment = await _service.GetByIdAsync(id, ct: HttpContext.RequestAborted);
            if (attachment == null) return NotFound();
            return attachment;
        }

        /// <summary>Creates a new attachment record (metadata only). File upload should be handled by a storage provider.</summary>
        /// <param name="request">The attachment data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<Attachment>> Create(AttachmentRequest request)
        {
            var created = await _service.CreateAsync(request, ct: HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Deletes an attachment record.</summary>
        /// <param name="id">The ID of the attachment to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id, ct: HttpContext.RequestAborted) ? NoContent() : NotFound();
    }
}

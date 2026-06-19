using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Application.Services;
using DailyNotes.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/memory-items")]
    [Authorize]
    public class MemoryItemsController : ApiControllerBase
    {
        private readonly IMemoryItemService _service;

        public MemoryItemsController(IMemoryItemService service)
        {
            _service = service;
        }

        /// <summary>Retrieves all memory items belonging to the current user/tenant.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemoryItem>>> GetAll()
            => Ok(await _service.GetAllAsync());

        /// <summary>Retrieves a specific memory item by its ID.</summary>
        /// <param name="id">The unique ID of the memory item.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<MemoryItem>> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        /// <summary>Creates a new memory item.</summary>
        /// <param name="request">The memory item data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<MemoryItem>> Create(MemoryItemRequest request)
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates an existing memory item.</summary>
        /// <param name="id">The ID of the memory item to update.</param>
        /// <param name="request">The updated memory item data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MemoryItemRequest request)
            => await _service.UpdateAsync(id, request) ? NoContent() : NotFound();

        /// <summary>Deletes a memory item.</summary>
        /// <param name="id">The ID of the memory item to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();

        /// <summary>Searches memory items using vector similarity.</summary>
        /// <param name="request">The search criteria including query embedding vector.</param>
        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<MemoryItem>>> Search(MemorySearchRequest request)
        {
            var results = await _service.SearchAsync(
                request.QueryEmbedding,
                request.MinImportanceScore,
                request.MemoryType,
                request.MemoryStatus,
                request.Limit);
            return Ok(results);
        }
    }
}

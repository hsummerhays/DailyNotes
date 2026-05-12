using DailyNotes.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/search")]
    [Authorize]
    public class SearchController : ApiControllerBase
    {
        private readonly ISearchService _service;

        public SearchController(ISearchService service)
        {
            _service = service;
        }

        /// <summary>Performs a cross-entity search across notes, tasks, and topics.</summary>
        /// <param name="q">The search query string.</param>
        /// <param name="type">The type of items to search for ('all', 'notes', 'tasks', or 'topics').</param>
        /// <param name="dateFrom">Optional start date filter for the search.</param>
        /// <param name="dateTo">Optional end date filter for the search.</param>
        /// <param name="projectId">Optional project ID filter.</param>
        /// <param name="statuses">Optional comma-separated status filter (e.g. 'pending,in_progress').</param>
        [HttpGet]
        public async Task<ActionResult<object>> Search(
            [FromQuery] string q,
            [FromQuery] string type = "all",
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] int? projectId = null,
            [FromQuery] string? statuses = null)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { message = "Search query 'q' is required." });

            return Ok(await _service.SearchAsync(q, type, dateFrom, dateTo, projectId, statuses));
        }
    }
}

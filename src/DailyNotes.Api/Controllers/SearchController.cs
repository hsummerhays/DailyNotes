using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/search")]
    [Authorize]
    public class SearchController : ApiControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public SearchController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>Performs a cross-entity search across notes, tasks, and topics.</summary>
        /// <param name="q">The search query string.</param>
        /// <param name="type">The type of items to search for ('all', 'notes', 'tasks', or 'topics').</param>
        /// <param name="dateFrom">Optional start date filter for the search.</param>
        /// <param name="dateTo">Optional end date filter for the search.</param>
        [HttpGet]
        public async Task<ActionResult<object>> Search(
            [FromQuery] string q,
            [FromQuery] string type = "all",
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { message = "Search query 'q' is required." });

            var tenantId = CurrentTenantId;
            var searchTerm = q.ToLower();
            var results = new Dictionary<string, object>();

            // Search Work Notes
            if (type == "all" || type == "notes")
            {
                var notesQuery = TenantScoped(_context.WorkNotes).AsQueryable();

                // Simple client-side like filtering for now as content is JSON
                // Ideally this would be specialized search index or DB function
                // For now just basic date filtering and perhaps fetching recent

                if (dateFrom.HasValue)
                {
                    var dFrom = DateOnly.FromDateTime(dateFrom.Value);
                    notesQuery = notesQuery.Where(n => n.NoteDate >= dFrom);
                }
                if (dateTo.HasValue)
                {
                    var dTo = DateOnly.FromDateTime(dateTo.Value);
                    notesQuery = notesQuery.Where(n => n.NoteDate <= dTo);
                }

                // If q is provided, we can't easily search JSON content in basic SQL efficiently without specific setup
                // We'll load recent notes and filter in memory for this simple implementation
                // OR we rely on a specific column if available.
                // Assuming we just return date-filtered list for now if no text search capability in DB

                var notes = await notesQuery
                    .OrderByDescending(n => n.NoteDate)
                    .Take(50)
                    .ToListAsync();

                // In-memory filter for 'q' on Content if needed
                if (!string.IsNullOrEmpty(q))
                {
                    var term = q.ToLower();
                    notes = notes.Where(n => n.Content.RootElement.ToString().ToLower().Contains(term)).ToList();
                }

                results["workNotes"] = notes;
            }

            // Search Work Tasks
            if (type == "all" || type == "tasks")
            {
                var tasksQuery = TenantScoped(_context.WorkTasks)
                    .Where(t => t.Name.ToLower().Contains(searchTerm));

                if (dateFrom.HasValue)
                    tasksQuery = tasksQuery.Where(t => t.CreatedAt >= dateFrom.Value);
                if (dateTo.HasValue)
                    tasksQuery = tasksQuery.Where(t => t.CreatedAt <= dateTo.Value);

                results["workTasks"] = await tasksQuery
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(50)
                    .ToListAsync();
            }

            // Search Topics
            if (type == "all" || type == "topics")
            {
                var topicsQuery = TenantScoped(_context.Topics)
                    .Where(t => t.Title.ToLower().Contains(searchTerm)
                             || (t.Description != null && t.Description.ToLower().Contains(searchTerm)));

                results["topics"] = await topicsQuery
                    .OrderBy(t => t.Title)
                    .Take(50)
                    .ToListAsync();

                // Also search topic notes (scoped through their parent topic)
                // We must ensure the topic itself is accessible to the user
                var accessibleTopicIds = TenantScoped(_context.Topics).Select(t => t.Id);

                var topicNotesQuery = _context.TopicNotes
                    .Where(n => (n.Title != null && n.Title.ToLower().Contains(searchTerm))
                        && accessibleTopicIds.Contains(n.TopicId));

                results["topicNotes"] = await topicNotesQuery
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(50)
                    .ToListAsync();
            }

            return results;
        }
    }
}

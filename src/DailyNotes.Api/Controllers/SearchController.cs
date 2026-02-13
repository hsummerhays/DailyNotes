using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/search")]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public SearchController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>GET /api/search?q=&type=all|notes|tasks|topics&dateFrom=&dateTo=&tags=</summary>
        [HttpGet]
        public async Task<ActionResult<object>> Search(
            [FromQuery] string q,
            [FromQuery] string type = "all",
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { message = "Search query 'q' is required." });

            var searchTerm = q.ToLower();
            var results = new Dictionary<string, object>();

            // Search Work Notes
            if (type == "all" || type == "notes")
            {
                var notesQuery = _context.WorkNotes.AsQueryable();

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

                // Basic text search on serialized content
                var notes = await notesQuery
                    .OrderByDescending(n => n.NoteDate)
                    .Take(50)
                    .ToListAsync();

                results["workNotes"] = notes;
            }

            // Search Work Tasks
            if (type == "all" || type == "tasks")
            {
                var tasksQuery = _context.WorkTasks
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
                var topicsQuery = _context.Topics
                    .Where(t => t.Title.ToLower().Contains(searchTerm)
                             || (t.Description != null && t.Description.ToLower().Contains(searchTerm)));

                results["topics"] = await topicsQuery
                    .OrderBy(t => t.Title)
                    .Take(50)
                    .ToListAsync();

                // Also search topic notes
                var topicNotesQuery = _context.TopicNotes
                    .Where(n => (n.Title != null && n.Title.ToLower().Contains(searchTerm)));

                results["topicNotes"] = await topicNotesQuery
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(50)
                    .ToListAsync();
            }

            return results;
        }
    }
}

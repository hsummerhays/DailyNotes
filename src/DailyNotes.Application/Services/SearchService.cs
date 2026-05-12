using DailyNotes.Core.Interfaces;
using DailyNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class SearchService : ApplicationServiceBase, ISearchService
    {
        public SearchService(DailyNotesDbContext db, ITenantContext tc) : base(db, tc) { }

        public async Task<Dictionary<string, object>> SearchAsync(
            string q,
            string type,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? projectId,
            string? statuses)
        {
            var searchTerm = q.ToLower();
            var results = new Dictionary<string, object>();

            if (type == "all" || type == "notes")
            {
                var notesQuery = TenantScoped(_db.WorkNotes).AsQueryable();

                if (dateFrom.HasValue)
                    notesQuery = notesQuery.Where(n => n.NoteDate >= DateOnly.FromDateTime(dateFrom.Value));
                if (dateTo.HasValue)
                    notesQuery = notesQuery.Where(n => n.NoteDate <= DateOnly.FromDateTime(dateTo.Value));

                if (projectId.HasValue)
                    notesQuery = notesQuery.Where(n => n.WorkTaskId != null &&
                        _db.WorkTasks.Any(t => t.Id == n.WorkTaskId && t.ProjectId == projectId.Value));

                if (!string.IsNullOrEmpty(statuses))
                {
                    var sList = statuses.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    notesQuery = notesQuery.Where(n => n.WorkTaskId != null &&
                        _db.WorkTasks.Any(t => t.Id == n.WorkTaskId && sList.Contains(t.Status)));
                }

                // Load recent notes then filter JSON content in memory
                var notes = await notesQuery.OrderByDescending(n => n.NoteDate).Take(50).ToListAsync();
                notes = notes.Where(n => n.Content.RootElement.ToString().ToLower().Contains(searchTerm)).ToList();
                results["workNotes"] = notes;
            }

            if (type == "all" || type == "tasks")
            {
                var tasksQuery = TenantScoped(_db.WorkTasks)
                    .Where(t => t.Name.ToLower().Contains(searchTerm));

                if (projectId.HasValue) tasksQuery = tasksQuery.Where(t => t.ProjectId == projectId.Value);

                if (!string.IsNullOrEmpty(statuses))
                {
                    var sList = statuses.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    tasksQuery = tasksQuery.Where(t => sList.Contains(t.Status));
                }

                if (dateFrom.HasValue) tasksQuery = tasksQuery.Where(t => t.CreatedAt >= dateFrom.Value);
                if (dateTo.HasValue) tasksQuery = tasksQuery.Where(t => t.CreatedAt <= dateTo.Value);

                results["workTasks"] = await tasksQuery.OrderByDescending(t => t.CreatedAt).Take(50).ToListAsync();
            }

            if (type == "all" || type == "topics")
            {
                var topicsQuery = TenantScoped(_db.Topics)
                    .Where(t => t.Title.ToLower().Contains(searchTerm)
                             || (t.Description != null && t.Description.ToLower().Contains(searchTerm)));

                results["topics"] = await topicsQuery.OrderBy(t => t.Title).Take(50).ToListAsync();

                var accessibleTopicIds = TenantScoped(_db.Topics).Select(t => t.Id);
                var topicNotesQuery = _db.TopicNotes
                    .Where(n => n.Title != null && n.Title.ToLower().Contains(searchTerm)
                        && accessibleTopicIds.Contains(n.TopicId));

                results["topicNotes"] = await topicNotesQuery.OrderByDescending(n => n.CreatedAt).Take(50).ToListAsync();
            }

            return results;
        }
    }
}

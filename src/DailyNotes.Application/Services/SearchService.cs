using DailyNotes.Application.Data;
using DailyNotes.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class SearchService : ApplicationServiceBase, ISearchService
    {
        public SearchService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock) : base(db, tc, clock) { }

        public async Task<Dictionary<string, object>> SearchAsync(
            string q,
            string type,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? projectId,
            string? statuses,
            CancellationToken ct = default)
        {
            var validTypes = new[] { "all", "notes", "tasks", "topics" };
            if (!validTypes.Contains(type))
                throw new DomainException($"Invalid search type '{type}'. Valid values: {string.Join(", ", validTypes)}.");

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

                notesQuery = notesQuery.Where(n =>
                    EF.Property<string>(n, "Content").ToLower().Contains(searchTerm));

                results["workNotes"] = await notesQuery.OrderByDescending(n => n.NoteDate).ToListAsync(ct);
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

                results["workTasks"] = await tasksQuery.OrderByDescending(t => t.CreatedAt).Take(50).ToListAsync(ct);
            }

            if (type == "all" || type == "topics")
            {
                var topicsQuery = TenantScoped(_db.Topics)
                    .Where(t => t.Title.ToLower().Contains(searchTerm)
                             || (t.Description != null && t.Description.ToLower().Contains(searchTerm)));

                results["topics"] = await topicsQuery.OrderBy(t => t.Title).Take(50).ToListAsync(ct);

                var accessibleTopicIds = TenantScoped(_db.Topics).Select(t => t.Id);
                var topicNotesQuery = _db.TopicNotes
                    .Where(n => n.Title != null && n.Title.ToLower().Contains(searchTerm)
                        && accessibleTopicIds.Contains(n.TopicId));

                results["topicNotes"] = await topicNotesQuery.OrderByDescending(n => n.CreatedAt).Take(50).ToListAsync(ct);
            }

            return results;
        }
    }
}

namespace DailyNotes.Application.Services
{
    public interface ISearchService
    {
        Task<Dictionary<string, object>> SearchAsync(
            string q,
            string type,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? projectId,
            string? statuses,
            CancellationToken ct = default);
    }
}

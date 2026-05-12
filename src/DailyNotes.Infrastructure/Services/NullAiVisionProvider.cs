using DailyNotes.Core.Interfaces;

namespace DailyNotes.Infrastructure.Services
{
    public class NullAiVisionProvider : IAiVisionProvider
    {
        public Task<string> AnalyzeImageAsync(Stream imageStream) => Task.FromResult(string.Empty);
        public Task<string> ExtractTextAsync(Stream imageStream) => Task.FromResult(string.Empty);
    }
}

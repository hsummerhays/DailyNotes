using System.IO;
using System.Threading.Tasks;

namespace DailyNotes.Core.Interfaces
{
    public interface IAiVisionProvider
    {
        Task<string> AnalyzeImageAsync(Stream imageStream);
        Task<string> ExtractTextAsync(Stream imageStream);
    }
}

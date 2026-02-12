using System.IO;
using System.Threading.Tasks;

namespace DailyNotes.Core.Interfaces
{
    public interface IFileStorageProvider
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName);
        Task<Stream> DownloadFileAsync(string fileUrl);
        Task DeleteFileAsync(string fileUrl);
    }
}

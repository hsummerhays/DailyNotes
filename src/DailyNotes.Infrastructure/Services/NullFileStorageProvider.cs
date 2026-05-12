using DailyNotes.Core.Interfaces;

namespace DailyNotes.Infrastructure.Services
{
    public class NullFileStorageProvider : IFileStorageProvider
    {
        public Task<string> UploadFileAsync(Stream fileStream, string fileName) => Task.FromResult(string.Empty);
        public Task<Stream> DownloadFileAsync(string fileUrl) => Task.FromResult<Stream>(Stream.Null);
        public Task DeleteFileAsync(string fileUrl) => Task.CompletedTask;
    }
}

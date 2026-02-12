using System.IO;
using System.Threading.Tasks;

namespace DailyNotes.Core.Interfaces
{
    public interface ISpeechProvider
    {
        Task<string> TranscribeAudioAsync(Stream audioStream);
        Task<byte[]> SynthesizeSpeechAsync(string text);
    }
}

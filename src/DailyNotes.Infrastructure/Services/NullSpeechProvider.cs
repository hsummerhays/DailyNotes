using DailyNotes.Core.Interfaces;

namespace DailyNotes.Infrastructure.Services
{
    public class NullSpeechProvider : ISpeechProvider
    {
        public Task<string> TranscribeAudioAsync(Stream audioStream) => Task.FromResult(string.Empty);
        public Task<byte[]> SynthesizeSpeechAsync(string text) => Task.FromResult(Array.Empty<byte>());
    }
}

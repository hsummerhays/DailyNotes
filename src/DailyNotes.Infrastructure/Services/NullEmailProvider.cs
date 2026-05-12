using DailyNotes.Core.Interfaces;

namespace DailyNotes.Infrastructure.Services
{
    public class NullEmailProvider : IEmailProvider
    {
        public Task SendEmailAsync(string to, string subject, string body) => Task.CompletedTask;
        public Task SendEmailWithAttachmentAsync(string to, string subject, string body, string attachPath) => Task.CompletedTask;
    }
}

using System.Threading.Tasks;

namespace DailyNotes.Core.Interfaces
{
    public interface IEmailProvider
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailWithAttachmentAsync(string to, string subject, string body, string attachPath);
    }
}

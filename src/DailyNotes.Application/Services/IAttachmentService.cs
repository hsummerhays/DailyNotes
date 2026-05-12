using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IAttachmentService
    {
        Task<IEnumerable<Attachment>> GetAllAsync(string? itemType, int? itemId);
        Task<Attachment?> GetByIdAsync(int id);
        Task<Attachment> CreateAsync(Attachment attachment);
        Task<bool> DeleteAsync(int id);
    }
}

using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IAttachmentService
    {
        Task<IEnumerable<Attachment>> GetAllAsync(string? itemType, int? itemId);
        Task<Attachment?> GetByIdAsync(int id);
        Task<Attachment> CreateAsync(AttachmentRequest request);
        Task<bool> DeleteAsync(int id);
    }
}

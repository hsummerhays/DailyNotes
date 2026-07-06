using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IAttachmentService
    {
        Task<IEnumerable<Attachment>> GetAllAsync(string? itemType, int? itemId, CancellationToken ct = default);
        Task<Attachment?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Attachment> CreateAsync(AttachmentRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}

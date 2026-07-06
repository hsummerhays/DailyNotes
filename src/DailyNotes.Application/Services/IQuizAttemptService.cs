using DailyNotes.Application.DTOs;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IQuizAttemptService
    {
        Task<IEnumerable<QuizAttempt>> GetAllAsync(int? quizId, CancellationToken ct = default);
        Task<QuizAttemptDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<QuizAttempt> SubmitAsync(QuizSubmissionDto submission, CancellationToken ct = default);
    }
}

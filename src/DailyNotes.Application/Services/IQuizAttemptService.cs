using DailyNotes.Application.DTOs;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IQuizAttemptService
    {
        Task<IEnumerable<QuizAttempt>> GetAllAsync(int? quizId);
        Task<QuizAttemptDetailDto?> GetByIdAsync(int id);
        Task<QuizAttempt> SubmitAsync(QuizSubmissionDto submission);
    }
}

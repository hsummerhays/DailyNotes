using DailyNotes.Application.DTOs;
using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IQuizService
    {
        Task<IEnumerable<Quiz>> GetAllAsync(int? topicId, int? difficulty, CancellationToken ct = default);
        Task<QuizDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Quiz> CreateAsync(QuizRequest request, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, QuizRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<QuizQuestion?> AddQuestionAsync(int quizId, QuizQuestionRequest request, CancellationToken ct = default);
        Task<QuizOption?> AddOptionAsync(int questionId, QuizOptionRequest request, CancellationToken ct = default);
    }
}

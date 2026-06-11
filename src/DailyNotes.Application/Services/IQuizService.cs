using DailyNotes.Application.DTOs;
using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IQuizService
    {
        Task<IEnumerable<Quiz>> GetAllAsync(int? topicId, int? difficulty);
        Task<QuizDetailDto?> GetByIdAsync(int id);
        Task<Quiz> CreateAsync(QuizRequest request);
        Task<bool> UpdateAsync(int id, QuizRequest request);
        Task<bool> DeleteAsync(int id);
        Task<QuizQuestion?> AddQuestionAsync(int quizId, QuizQuestionRequest request);
        Task<QuizOption?> AddOptionAsync(int questionId, QuizOptionRequest request);
    }
}

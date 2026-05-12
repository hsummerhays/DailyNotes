using DailyNotes.Application.DTOs;
using DailyNotes.Core.Entities;

namespace DailyNotes.Application.Services
{
    public interface IQuizService
    {
        Task<IEnumerable<Quiz>> GetAllAsync(int? topicId, int? difficulty);
        Task<QuizDetailDto?> GetByIdAsync(int id);
        Task<Quiz> CreateAsync(Quiz quiz);
        Task<bool> UpdateAsync(int id, Quiz quiz);
        Task<bool> DeleteAsync(int id);
        Task<QuizQuestion?> AddQuestionAsync(int quizId, QuizQuestion question);
        Task<QuizOption?> AddOptionAsync(int questionId, QuizOption option);
    }
}

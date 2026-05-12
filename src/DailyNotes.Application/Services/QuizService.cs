using DailyNotes.Application.DTOs;
using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class QuizService : ApplicationServiceBase, IQuizService
    {
        public QuizService(DailyNotesDbContext db, ITenantContext tc) : base(db, tc) { }

        public async Task<IEnumerable<Quiz>> GetAllAsync(int? topicId, int? difficulty)
        {
            var query = TenantOnlyScoped(_db.Quizzes).AsQueryable();

            if (topicId.HasValue) query = query.Where(q => q.TopicId == topicId.Value);
            if (difficulty.HasValue) query = query.Where(q => q.Difficulty == difficulty.Value);

            return await query.OrderByDescending(q => q.CreatedAt).ToListAsync();
        }

        public async Task<QuizDetailDto?> GetByIdAsync(int id)
        {
            var quiz = await TenantOnlyScoped(_db.Quizzes).FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null) return null;

            var questions = await _db.QuizQuestions
                .Where(q => q.QuizId == id)
                .OrderBy(q => q.SortOrder)
                .ToListAsync();

            var questionIds = questions.Select(q => q.Id).ToList();
            var options = await _db.QuizOptions
                .Where(o => questionIds.Contains(o.QuestionId))
                .OrderBy(o => o.SortOrder)
                .ToListAsync();

            return new QuizDetailDto
            {
                Quiz = quiz,
                Questions = questions.Select(q => new QuizQuestionDetailDto
                {
                    Question = q,
                    Options = options.Where(o => o.QuestionId == q.Id).ToList()
                })
            };
        }

        public async Task<Quiz> CreateAsync(Quiz quiz)
        {
            quiz.TenantId = _tc.TenantId;
            quiz.CreatedAt = DateTime.UtcNow;

            _db.Quizzes.Add(quiz);
            await _db.SaveChangesAsync();
            return quiz;
        }

        public async Task<bool> UpdateAsync(int id, Quiz quiz)
        {
            var existing = await TenantOnlyScoped(_db.Quizzes).FirstOrDefaultAsync(q => q.Id == id);
            if (existing == null) return false;

            existing.Title = quiz.Title;
            existing.TopicId = quiz.TopicId;
            existing.Difficulty = quiz.Difficulty;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var quiz = await TenantOnlyScoped(_db.Quizzes).FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null) return false;

            _db.Quizzes.Remove(quiz);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<QuizQuestion?> AddQuestionAsync(int quizId, QuizQuestion question)
        {
            var quiz = await TenantOnlyScoped(_db.Quizzes).FirstOrDefaultAsync(q => q.Id == quizId);
            if (quiz == null) return null;

            question.QuizId = quizId;
            _db.QuizQuestions.Add(question);
            await _db.SaveChangesAsync();
            return question;
        }

        public async Task<QuizOption?> AddOptionAsync(int questionId, QuizOption option)
        {
            var question = await _db.QuizQuestions.FindAsync(questionId);
            if (question == null) return null;

            // Verify the quiz belongs to the current tenant
            var quizAccessible = await TenantOnlyScoped(_db.Quizzes).AnyAsync(q => q.Id == question.QuizId);
            if (!quizAccessible) return null;

            option.QuestionId = questionId;
            _db.QuizOptions.Add(option);
            await _db.SaveChangesAsync();
            return option;
        }
    }
}

using DailyNotes.Application.Data;
using DailyNotes.Application.DTOs;
using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class QuizService : ApplicationServiceBase, IQuizService
    {
        public QuizService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock) : base(db, tc, clock) { }

        public async Task<IEnumerable<Quiz>> GetAllAsync(int? topicId, int? difficulty, CancellationToken ct = default)
        {
            var query = TenantOnlyScoped(_db.Quizzes).AsQueryable();

            if (topicId.HasValue) query = query.Where(q => q.TopicId == topicId.Value);
            if (difficulty.HasValue) query = query.Where(q => q.Difficulty == difficulty.Value);

            return await query.OrderByDescending(q => q.CreatedAt).ToListAsync(ct);
        }

        public async Task<QuizDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var quiz = await TenantOnlyScoped(_db.Quizzes).FirstOrDefaultAsync(q => q.Id == id, ct);
            if (quiz == null) return null;

            var questions = await _db.QuizQuestions
                .Where(q => q.QuizId == id)
                .OrderBy(q => q.SortOrder)
                .ToListAsync(ct);

            var questionIds = questions.Select(q => q.Id).ToList();
            var options = await _db.QuizOptions
                .Where(o => questionIds.Contains(o.QuestionId))
                .OrderBy(o => o.SortOrder)
                .ToListAsync(ct);

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

        public async Task<Quiz> CreateAsync(QuizRequest request, CancellationToken ct = default)
        {
            var quiz = new Quiz
            {
                TenantId = _tc.TenantId,
                TopicId = request.TopicId,
                Title = request.Title,
                Difficulty = request.Difficulty,
                CreatedAt = _clock.GetUtcNow().UtcDateTime
            };

            _db.Quizzes.Add(quiz);
            await _db.SaveChangesAsync(ct);
            return quiz;
        }

        public async Task<bool> UpdateAsync(int id, QuizRequest request, CancellationToken ct = default)
        {
            var existing = await TenantOnlyScoped(_db.Quizzes).FirstOrDefaultAsync(q => q.Id == id, ct);
            if (existing == null) return false;

            existing.Title = request.Title;
            existing.TopicId = request.TopicId;
            existing.Difficulty = request.Difficulty;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var quiz = await TenantOnlyScoped(_db.Quizzes).FirstOrDefaultAsync(q => q.Id == id, ct);
            if (quiz == null) return false;

            _db.Quizzes.Remove(quiz);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<QuizQuestion?> AddQuestionAsync(int quizId, QuizQuestionRequest request, CancellationToken ct = default)
        {
            var quiz = await TenantOnlyScoped(_db.Quizzes).FirstOrDefaultAsync(q => q.Id == quizId, ct);
            if (quiz == null) return null;

            var question = new QuizQuestion
            {
                QuizId = quizId,
                QuestionText = request.QuestionText,
                Explanation = request.Explanation,
                SortOrder = request.SortOrder
            };

            _db.QuizQuestions.Add(question);
            await _db.SaveChangesAsync(ct);
            return question;
        }

        public async Task<QuizOption?> AddOptionAsync(int questionId, QuizOptionRequest request, CancellationToken ct = default)
        {
            var question = await _db.QuizQuestions.FindAsync(new object[] { questionId }, ct);
            if (question == null) return null;

            // Verify the quiz belongs to the current tenant
            var quizAccessible = await TenantOnlyScoped(_db.Quizzes).AnyAsync(q => q.Id == question.QuizId, ct);
            if (!quizAccessible) return null;

            var option = new QuizOption
            {
                QuestionId = questionId,
                OptionText = request.OptionText,
                IsCorrect = request.IsCorrect,
                SortOrder = request.SortOrder
            };

            _db.QuizOptions.Add(option);
            await _db.SaveChangesAsync(ct);
            return option;
        }
    }
}

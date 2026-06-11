using DailyNotes.Application.Data;
using DailyNotes.Application.DTOs;
using DailyNotes.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class QuizAttemptService : ApplicationServiceBase, IQuizAttemptService
    {
        public QuizAttemptService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock) : base(db, tc, clock) { }

        public async Task<IEnumerable<QuizAttempt>> GetAllAsync(int? quizId)
        {
            // Filter by both UserId and TenantId (via quiz membership) for consistency
            var query = _db.QuizAttempts
                .Where(a => a.UserId == _tc.UserId
                    && _db.Quizzes.Any(q => q.Id == a.QuizId && q.TenantId == _tc.TenantId))
                .AsQueryable();

            if (quizId.HasValue) query = query.Where(a => a.QuizId == quizId.Value);

            return await query.OrderByDescending(a => a.StartedAt).ToListAsync();
        }

        public async Task<QuizAttemptDetailDto?> GetByIdAsync(int id)
        {
            // Bug A fix: scope to current user
            var attempt = await _db.QuizAttempts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == _tc.UserId);
            if (attempt == null) return null;

            var answers = await _db.QuizAnswers.Where(a => a.AttemptId == id).ToListAsync();

            return new QuizAttemptDetailDto { Attempt = attempt, Answers = answers };
        }

        public async Task<QuizAttempt> SubmitAsync(QuizSubmissionDto submission)
        {
            var quiz = await _db.Quizzes
                .FirstOrDefaultAsync(q => q.Id == submission.QuizId && q.TenantId == _tc.TenantId);
            if (quiz == null)
                throw new KeyNotFoundException($"Quiz {submission.QuizId} not found.");

            using var transaction = await _db.Database.BeginTransactionAsync();

            var attempt = new QuizAttempt
            {
                QuizId = submission.QuizId,
                UserId = _tc.UserId,
                StartedAt = _clock.GetUtcNow().UtcDateTime
            };

            _db.QuizAttempts.Add(attempt);
            await _db.SaveChangesAsync();

            int correct = 0;
            int total = submission.Answers.Count;

            foreach (var answer in submission.Answers)
            {
                var option = await _db.QuizOptions.FindAsync(answer.SelectedOptionId);

                // Bug B fix: verify the option belongs to the submitted question
                bool isCorrect = option?.IsCorrect == true && option?.QuestionId == answer.QuestionId;
                if (isCorrect) correct++;

                _db.QuizAnswers.Add(new QuizAnswer
                {
                    AttemptId = attempt.Id,
                    QuestionId = answer.QuestionId,
                    SelectedOptionId = answer.SelectedOptionId,
                    IsCorrect = isCorrect
                });
            }

            attempt.Score = total > 0 ? (decimal)correct / total * 100 : 0;
            attempt.CompletedAt = _clock.GetUtcNow().UtcDateTime;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return attempt;
        }
    }
}

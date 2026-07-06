using DailyNotes.Application.Data;
using DailyNotes.Application.DTOs;
using DailyNotes.Core.Entities;
using DailyNotes.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Application.Services
{
    public class QuizAttemptService : ApplicationServiceBase, IQuizAttemptService
    {
        public QuizAttemptService(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock) : base(db, tc, clock) { }

        public async Task<IEnumerable<QuizAttempt>> GetAllAsync(int? quizId, CancellationToken ct = default)
        {
            var query = _db.QuizAttempts
                .Where(a => a.TenantId == _tc.TenantId && a.UserId == _tc.UserId)
                .AsQueryable();

            if (quizId.HasValue) query = query.Where(a => a.QuizId == quizId.Value);

            return await query.OrderByDescending(a => a.StartedAt).ToListAsync(ct);
        }

        public async Task<QuizAttemptDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var attempt = await _db.QuizAttempts
                .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == _tc.TenantId && a.UserId == _tc.UserId, ct);
            if (attempt == null) return null;

            var answers = await _db.QuizAnswers.Where(a => a.AttemptId == id).ToListAsync(ct);

            return new QuizAttemptDetailDto { Attempt = attempt, Answers = answers };
        }

        public async Task<QuizAttempt> SubmitAsync(QuizSubmissionDto submission, CancellationToken ct = default)
        {
            var quiz = await _db.Quizzes
                .FirstOrDefaultAsync(q => q.Id == submission.QuizId && q.TenantId == _tc.TenantId, ct);
            if (quiz == null)
                throw new KeyNotFoundException($"Quiz {submission.QuizId} not found.");

            // Verify every submitted question and option belongs to this quiz
            var validQuestionIds = await _db.QuizQuestions
                .Where(q => q.QuizId == submission.QuizId)
                .Select(q => q.Id)
                .ToHashSetAsync(ct);

            var validOptionIds = await _db.QuizOptions
                .Where(o => validQuestionIds.Contains(o.QuestionId))
                .Select(o => o.Id)
                .ToHashSetAsync(ct);

            foreach (var answer in submission.Answers)
            {
                if (!validQuestionIds.Contains(answer.QuestionId) || !validOptionIds.Contains(answer.SelectedOptionId))
                    throw new DomainException("Answer references a question or option that does not belong to this quiz.", 422);
            }

            using var transaction = await _db.Database.BeginTransactionAsync(ct);

            var attempt = new QuizAttempt
            {
                TenantId = _tc.TenantId,
                QuizId = submission.QuizId,
                UserId = _tc.UserId,
                StartedAt = _clock.GetUtcNow().UtcDateTime
            };

            _db.QuizAttempts.Add(attempt);
            await _db.SaveChangesAsync(ct);

            int correct = 0;
            int total = submission.Answers.Count;

            foreach (var answer in submission.Answers)
            {
                var option = await _db.QuizOptions.FindAsync(new object[] { answer.SelectedOptionId }, ct);
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

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return attempt;
        }
    }
}

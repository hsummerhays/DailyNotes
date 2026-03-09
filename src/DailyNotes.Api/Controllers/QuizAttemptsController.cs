using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/quiz-attempts")]
    [Authorize]
    public class QuizAttemptsController : ApiControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public QuizAttemptsController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>Retrieves the quiz attempt history for the current user, optionally filtered by quiz ID.</summary>
        /// <param name="quizId">Filter history by a specific quiz.</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizAttempt>>> GetAll([FromQuery] int? quizId)
        {
            var userId = CurrentUserId;
            var query = _context.QuizAttempts
                .Where(a => a.UserId == userId)
                .AsQueryable();

            if (quizId.HasValue)
                query = query.Where(a => a.QuizId == quizId.Value);

            return await query.OrderByDescending(a => a.StartedAt).ToListAsync();
        }

        /// <summary>Retrieves a specific quiz attempt by its ID, including the user's submitted answers.</summary>
        /// <param name="id">The unique ID of the quiz attempt.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var attempt = await _context.QuizAttempts.FindAsync(id);
            if (attempt == null)
                return NotFound();

            var answers = await _context.QuizAnswers
                .Where(a => a.AttemptId == id)
                .ToListAsync();

            return new { Attempt = attempt, Answers = answers };
        }

        /// <summary>Submits a completed quiz attempt with all user answers for scoring.</summary>
        /// <param name="submission">The quiz submission data containing the quiz ID and chosen options.</param>
        [HttpPost]
        public async Task<ActionResult<QuizAttempt>> Submit([FromBody] QuizSubmissionDto submission)
        {
            var tenantId = CurrentTenantId;
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == submission.QuizId && q.TenantId == tenantId);
            if (quiz == null)
                return NotFound(new { message = "Quiz not found." });

            var attempt = new QuizAttempt
            {
                QuizId = submission.QuizId,
                UserId = CurrentUserId,
                StartedAt = DateTime.UtcNow
            };

            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            int correct = 0;
            int total = submission.Answers.Count;

            foreach (var answer in submission.Answers)
            {
                var option = await _context.QuizOptions.FindAsync(answer.SelectedOptionId);
                bool isCorrect = option?.IsCorrect ?? false;
                if (isCorrect) correct++;

                _context.QuizAnswers.Add(new QuizAnswer
                {
                    AttemptId = attempt.Id,
                    QuestionId = answer.QuestionId,
                    SelectedOptionId = answer.SelectedOptionId,
                    IsCorrect = isCorrect
                });
            }

            attempt.Score = total > 0 ? (decimal)correct / total * 100 : 0;
            attempt.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = attempt.Id }, attempt);
        }
    }

    public class QuizSubmissionDto
    {
        public int QuizId { get; set; }
        public List<QuizAnswerDto> Answers { get; set; } = new();
    }

    public class QuizAnswerDto
    {
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
    }
}

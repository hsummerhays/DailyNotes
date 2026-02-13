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
    public class QuizAttemptsController : ControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public QuizAttemptsController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>GET /api/quiz-attempts?quizId= (history)</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizAttempt>>> GetAll([FromQuery] int? quizId)
        {
            var query = _context.QuizAttempts.AsQueryable();

            if (quizId.HasValue)
                query = query.Where(a => a.QuizId == quizId.Value);

            return await query.OrderByDescending(a => a.StartedAt).ToListAsync();
        }

        /// <summary>GET /api/quiz-attempts/{id} — attempt with answers</summary>
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

        /// <summary>POST /api/quiz-attempts — submit a quiz attempt with answers</summary>
        [HttpPost]
        public async Task<ActionResult<QuizAttempt>> Submit([FromBody] QuizSubmissionDto submission)
        {
            var quiz = await _context.Quizzes.FindAsync(submission.QuizId);
            if (quiz == null)
                return NotFound(new { message = "Quiz not found." });

            var attempt = new QuizAttempt
            {
                QuizId = submission.QuizId,
                UserId = submission.UserId,
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
        public string UserId { get; set; } = string.Empty;
        public List<QuizAnswerDto> Answers { get; set; } = new();
    }

    public class QuizAnswerDto
    {
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
    }
}

using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/quizzes")]
    [Authorize]
    public class QuizzesController : ApiControllerBase
    {
        private readonly DailyNotesDbContext _context;

        public QuizzesController(DailyNotesDbContext context)
        {
            _context = context;
        }

        /// <summary>GET /api/quizzes?topicId=&difficulty=</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetAll(
            [FromQuery] int? topicId,
            [FromQuery] int? difficulty)
        {
            var tenantId = CurrentTenantId;
            var query = _context.Quizzes
                .Where(q => q.TenantId == tenantId)
                .AsQueryable();

            if (topicId.HasValue)
                query = query.Where(q => q.TopicId == topicId.Value);

            if (difficulty.HasValue)
                query = query.Where(q => q.Difficulty == difficulty.Value);

            return await query.OrderByDescending(q => q.CreatedAt).ToListAsync();
        }

        /// <summary>GET /api/quizzes/{id} — full quiz with questions and options</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var tenantId = CurrentTenantId;
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id && q.TenantId == tenantId);
            if (quiz == null)
                return NotFound();

            var questions = await _context.QuizQuestions
                .Where(q => q.QuizId == id)
                .OrderBy(q => q.SortOrder)
                .ToListAsync();

            var questionIds = questions.Select(q => q.Id).ToList();
            var options = await _context.QuizOptions
                .Where(o => questionIds.Contains(o.QuestionId))
                .OrderBy(o => o.SortOrder)
                .ToListAsync();

            return new
            {
                Quiz = quiz,
                Questions = questions.Select(q => new
                {
                    Question = q,
                    Options = options.Where(o => o.QuestionId == q.Id).ToList()
                })
            };
        }

        [HttpPost]
        public async Task<ActionResult<Quiz>> Create(Quiz quiz)
        {
            quiz.TenantId = CurrentTenantId;
            quiz.CreatedAt = DateTime.UtcNow;

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = quiz.Id }, quiz);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Quiz quiz)
        {
            if (id != quiz.Id)
                return BadRequest(new { message = "ID mismatch." });

            var tenantId = CurrentTenantId;
            var existing = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id && q.TenantId == tenantId);
            if (existing == null)
                return NotFound();

            existing.Title = quiz.Title;
            existing.TopicId = quiz.TopicId;
            existing.Difficulty = quiz.Difficulty;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = CurrentTenantId;
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id && q.TenantId == tenantId);
            if (quiz == null)
                return NotFound();

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>POST /api/quizzes/{quizId}/questions</summary>
        [HttpPost("{quizId}/questions")]
        public async Task<ActionResult<QuizQuestion>> AddQuestion(int quizId, QuizQuestion question)
        {
            var tenantId = CurrentTenantId;
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == quizId && q.TenantId == tenantId);
            if (quiz == null)
                return NotFound(new { message = "Quiz not found." });

            question.QuizId = quizId;
            _context.QuizQuestions.Add(question);
            await _context.SaveChangesAsync();

            return Ok(question);
        }

        /// <summary>POST /api/quizzes/questions/{questionId}/options</summary>
        [HttpPost("questions/{questionId}/options")]
        public async Task<ActionResult<QuizOption>> AddOption(int questionId, QuizOption option)
        {
            var question = await _context.QuizQuestions.FindAsync(questionId);
            if (question == null)
                return NotFound(new { message = "Question not found." });

            option.QuestionId = questionId;
            _context.QuizOptions.Add(option);
            await _context.SaveChangesAsync();

            return Ok(option);
        }
    }
}

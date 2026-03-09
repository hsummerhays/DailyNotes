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

        /// <summary>Retrieves all quizzes, optionally filtered by topic or difficulty.</summary>
        /// <param name="topicId">Filter quizzes by a specific topic.</param>
        /// <param name="difficulty">Filter quizzes by difficulty level (1-5).</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetAll(
            [FromQuery] int? topicId,
            [FromQuery] int? difficulty)
        {
            var query = TenantOnlyScoped(_context.Quizzes).AsQueryable();

            if (topicId.HasValue)
                query = query.Where(q => q.TopicId == topicId.Value);

            if (difficulty.HasValue)
                query = query.Where(q => q.Difficulty == difficulty.Value);

            return await query.OrderByDescending(q => q.CreatedAt).ToListAsync();
        }

        /// <summary>Retrieves a full quiz by its ID, including all questions and available options.</summary>
        /// <param name="id">The unique ID of the quiz.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var quiz = await TenantOnlyScoped(_context.Quizzes)
                .FirstOrDefaultAsync(q => q.Id == id);
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

        /// <summary>Creates a new quiz.</summary>
        /// <param name="quiz">The quiz data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<Quiz>> Create(Quiz quiz)
        {
            quiz.TenantId = CurrentTenantId;
            quiz.CreatedAt = DateTime.UtcNow;

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = quiz.Id }, quiz);
        }

        /// <summary>Updates an existing quiz.</summary>
        /// <param name="id">The ID of the quiz to update.</param>
        /// <param name="quiz">The updated quiz data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Quiz quiz)
        {
            if (id != quiz.Id)
                return BadRequest(new { message = "ID mismatch." });

            var existing = await TenantOnlyScoped(_context.Quizzes)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (existing == null)
                return NotFound();

            existing.Title = quiz.Title;
            existing.TopicId = quiz.TopicId;
            existing.Difficulty = quiz.Difficulty;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Deletes a quiz and all its associated questions and options.</summary>
        /// <param name="id">The ID of the quiz to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var quiz = await TenantOnlyScoped(_context.Quizzes)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null)
                return NotFound();

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Adds a new question to a specific quiz.</summary>
        /// <param name="quizId">The ID of the quiz.</param>
        /// <param name="question">The question data to be added.</param>
        [HttpPost("{quizId}/questions")]
        public async Task<ActionResult<QuizQuestion>> AddQuestion(int quizId, QuizQuestion question)
        {
            var quiz = await TenantOnlyScoped(_context.Quizzes)
                .FirstOrDefaultAsync(q => q.Id == quizId);
            if (quiz == null)
                return NotFound(new { message = "Quiz not found." });

            question.QuizId = quizId;
            _context.QuizQuestions.Add(question);
            await _context.SaveChangesAsync();

            return Ok(question);
        }

        /// <summary>Adds a new option to a specific quiz question.</summary>
        /// <param name="questionId">The ID of the quiz question.</param>
        /// <param name="option">The option data to be added.</param>
        [HttpPost("questions/{questionId}/options")]
        public async Task<ActionResult<QuizOption>> AddOption(int questionId, QuizOption option)
        {
            var question = await _context.QuizQuestions.FindAsync(questionId);
            if (question == null)
                return NotFound(new { message = "Question not found." });

            // Security check: ensure the quiz belongs to the current tenant
            var quiz = await TenantOnlyScoped(_context.Quizzes)
                .FirstOrDefaultAsync(q => q.Id == question.QuizId);
            if (quiz == null)
                return NotFound(new { message = "Quiz not found or access denied." });

            option.QuestionId = questionId;
            _context.QuizOptions.Add(option);
            await _context.SaveChangesAsync();

            return Ok(option);
        }
    }
}

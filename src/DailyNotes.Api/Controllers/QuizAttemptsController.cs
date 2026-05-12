using DailyNotes.Application.DTOs;
using DailyNotes.Application.Services;
using DailyNotes.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/quiz-attempts")]
    [Authorize]
    public class QuizAttemptsController : ApiControllerBase
    {
        private readonly IQuizAttemptService _service;

        public QuizAttemptsController(IQuizAttemptService service)
        {
            _service = service;
        }

        /// <summary>Retrieves the quiz attempt history for the current user, optionally filtered by quiz ID.</summary>
        /// <param name="quizId">Filter history by a specific quiz.</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizAttempt>>> GetAll([FromQuery] int? quizId)
            => Ok(await _service.GetAllAsync(quizId));

        /// <summary>Retrieves a specific quiz attempt by its ID, including the user's submitted answers.</summary>
        /// <param name="id">The unique ID of the quiz attempt.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var detail = await _service.GetByIdAsync(id);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        /// <summary>Submits a completed quiz attempt with all user answers for scoring.</summary>
        /// <param name="submission">The quiz submission data containing the quiz ID and chosen options.</param>
        [HttpPost]
        public async Task<ActionResult<QuizAttempt>> Submit([FromBody] QuizSubmissionDto submission)
        {
            try
            {
                var attempt = await _service.SubmitAsync(submission);
                return CreatedAtAction(nameof(GetById), new { id = attempt.Id }, attempt);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}

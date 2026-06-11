using DailyNotes.Application.DTOs.Requests;
using DailyNotes.Application.Services;
using DailyNotes.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/quizzes")]
    [Authorize]
    public class QuizzesController : ApiControllerBase
    {
        private readonly IQuizService _service;

        public QuizzesController(IQuizService service)
        {
            _service = service;
        }

        /// <summary>Retrieves all quizzes, optionally filtered by topic or difficulty.</summary>
        /// <param name="topicId">Filter quizzes by a specific topic.</param>
        /// <param name="difficulty">Filter quizzes by difficulty level (1-5).</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetAll(
            [FromQuery] int? topicId,
            [FromQuery] int? difficulty)
            => Ok(await _service.GetAllAsync(topicId, difficulty));

        /// <summary>Retrieves a full quiz by its ID, including all questions and available options.</summary>
        /// <param name="id">The unique ID of the quiz.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var detail = await _service.GetByIdAsync(id);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        /// <summary>Creates a new quiz.</summary>
        /// <param name="request">The quiz data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<Quiz>> Create(QuizRequest request)
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates an existing quiz.</summary>
        /// <param name="id">The ID of the quiz to update.</param>
        /// <param name="request">The updated quiz data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, QuizRequest request)
            => await _service.UpdateAsync(id, request) ? NoContent() : NotFound();

        /// <summary>Deletes a quiz and all its associated questions and options.</summary>
        /// <param name="id">The ID of the quiz to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();

        /// <summary>Adds a new question to a specific quiz.</summary>
        /// <param name="quizId">The ID of the quiz.</param>
        /// <param name="request">The question data to be added.</param>
        [HttpPost("{quizId}/questions")]
        public async Task<ActionResult<QuizQuestion>> AddQuestion(int quizId, QuizQuestionRequest request)
        {
            var result = await _service.AddQuestionAsync(quizId, request);
            if (result == null) return NotFound(new { message = "Quiz not found." });
            return Ok(result);
        }

        /// <summary>Adds a new option to a specific quiz question.</summary>
        /// <param name="questionId">The ID of the quiz question.</param>
        /// <param name="request">The option data to be added.</param>
        [HttpPost("questions/{questionId}/options")]
        public async Task<ActionResult<QuizOption>> AddOption(int questionId, QuizOptionRequest request)
        {
            var result = await _service.AddOptionAsync(questionId, request);
            if (result == null) return NotFound(new { message = "Question not found or access denied." });
            return Ok(result);
        }
    }
}

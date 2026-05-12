using DailyNotes.Application.Services;
using DailyNotes.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/courses")]
    [Authorize]
    public class CoursesController : ApiControllerBase
    {
        private readonly ICourseService _service;

        public CoursesController(ICourseService service)
        {
            _service = service;
        }

        /// <summary>Retrieves all courses, optionally filtered by semester.</summary>
        /// <param name="semester">Filter courses by a specific semester (e.g., 'Spring 2026').</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetAll([FromQuery] string? semester)
            => Ok(await _service.GetAllAsync(semester));

        /// <summary>Retrieves a specific course by its ID, including its associated assignments.</summary>
        /// <param name="id">The unique ID of the course.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetById(int id)
        {
            var course = await _service.GetByIdAsync(id);
            if (course == null) return NotFound();
            return course;
        }

        /// <summary>Creates a new course.</summary>
        /// <param name="course">The course data to be created.</param>
        [HttpPost]
        public async Task<ActionResult<Course>> Create(Course course)
        {
            var created = await _service.CreateAsync(course);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates an existing course record.</summary>
        /// <param name="id">The ID of the course to update.</param>
        /// <param name="course">The updated course data.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Course course)
        {
            if (id != course.Id) return BadRequest(new { message = "ID mismatch." });
            return await _service.UpdateAsync(id, course) ? NoContent() : NotFound();
        }

        /// <summary>Deletes a course record.</summary>
        /// <param name="id">The ID of the course to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}

using LearnToDev.Api.DTOs;
using LearnToDev.Api.Models;
using LearnToDev.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnToDev.Api.Controllers;

// CRUD for study questions with answer options. Only admins: the role check is on the class, so it covers every action.
// [ApiController] runs the validation in SaveQuestionDto (including Validate) and answers 400 automatically
[ApiController]
[Route("api/questions")]
[Authorize(Roles = Roles.Admin)]
public class QuestionsController : ControllerBase
{
    private readonly IStudyQuestionRepository _questions;
    private readonly ISchoolRepository _schools;
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(IStudyQuestionRepository questions, ISchoolRepository schools,
        ILogger<QuestionsController> logger)
    {
        _questions = questions;
        _schools = schools;
        _logger = logger;
    }

    // GET api/questions?search=html&schoolId=1 -> 200 with the questions that match.
    // Both parameters are optional, so without them it returns all questions
    [HttpGet]
    public async Task<ActionResult<List<QuestionDto>>> GetAll([FromQuery] string? search, [FromQuery] int? schoolId)
    {
        var questions = await _questions.SearchAsync(search, schoolId);
        return Ok(questions.Select(q => q.ToDto()).ToList());
    }

    // GET api/questions/5 -> 200 with the question, or 404
    [HttpGet("{id:int}")]
    public async Task<ActionResult<QuestionDto>> GetById(int id)
    {
        var question = await _questions.GetByIdAsync(id);
        if (question == null)
            return NotFound();

        return Ok(question.ToDto());
    }

    // POST api/questions -> 201 with a Location header to the new question, or 400
    [HttpPost]
    public async Task<ActionResult<QuestionDto>> Create(SaveQuestionDto dto)
    {
        if (!await SchoolExistsAsync(dto.SchoolId))
            return ValidationProblem(ModelState);

        var created = await _questions.AddAsync(dto.ToModel());
        _logger.LogInformation("Question {QuestionId} created in school {SchoolId}", created.Id, created.SchoolId);

        // Fetch again so School is loaded, otherwise ToDto fails on School.Name
        var saved = await _questions.GetByIdAsync(created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, saved!.ToDto());
    }

    // PUT api/questions/5 -> 204, 400 or 404. The id comes from the URL, not the body
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveQuestionDto dto)
    {
        if (!await SchoolExistsAsync(dto.SchoolId))
            return ValidationProblem(ModelState);

        var question = dto.ToModel();
        question.Id = id;

        if (!await _questions.UpdateAsync(question))
        {
            _logger.LogWarning("Tried to update question {QuestionId}, which does not exist", id);
            return NotFound();
        }

        _logger.LogInformation("Question {QuestionId} updated", id);
        return NoContent();
    }

    // DELETE api/questions/5 -> 204 or 404
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _questions.DeleteAsync(id))
        {
            _logger.LogWarning("Tried to delete question {QuestionId}, which does not exist", id);
            return NotFound();
        }

        _logger.LogInformation("Question {QuestionId} deleted", id);
        return NoContent();
    }

    // An unknown SchoolId is the client's fault, so it should be 400 and not a 500 from the foreign key.
    // Adds the error to ModelState so it comes in the same format as the other validation errors
    private async Task<bool> SchoolExistsAsync(int schoolId)
    {
        if (await _schools.GetByIdAsync(schoolId) != null)
            return true;
        
        _logger.LogWarning("Rejected question with unknown school {SchoolId}", schoolId);
        ModelState.AddModelError(nameof(SaveQuestionDto.SchoolId), "Fant ikke skolen.");
        return false;
    }
}

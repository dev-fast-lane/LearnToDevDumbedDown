using LearnToDev.Api.DTOs;
using LearnToDev.Api.Models;
using LearnToDev.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnToDev.Api.Controllers;

//CRUD and search for questions. Admin only, since the questions show the correct answer (R-01, R-05)
[ApiController]
[Route("api/questions")]
[Authorize(Roles = Roles.Admin)]
public class QuestionsController : ControllerBase
{
    private readonly IStudyQuestionRepository _questionRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(IStudyQuestionRepository questionRepository, ISchoolRepository schoolRepository,
        ILogger<QuestionsController> logger)
    {
        _questionRepository = questionRepository;
        _schoolRepository = schoolRepository;
        _logger = logger;
    }

    //GET api/questions?search=html&schoolId=1. Both parameters are optional
    [HttpGet]
    public async Task<ActionResult<List<QuestionDto>>> Search([FromQuery] string? search, [FromQuery] int? schoolId)
    {
        var questions = await _questionRepository.SearchAsync(search, schoolId);
        return Ok(questions.Select(q => q.ToDto()).ToList());
    }

    //GET api/questions/3
    [HttpGet("{id:int}")]
    public async Task<ActionResult<QuestionDto>> GetById(int id)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question == null)
            return NotFound();

        return Ok(question.ToDto());
    }

    //POST api/questions
    [HttpPost]
    public async Task<ActionResult<QuestionDto>> Create(SaveQuestionDto dto)
    {
        if (await _schoolRepository.GetByIdAsync(dto.SchoolId) == null)
            return SchoolDoesNotExist();

        var question = await _questionRepository.AddAsync(dto.ToModel());
        _logger.LogInformation("Question {QuestionId} created", question.Id);

        //Fetch it again, so the school name comes with it
        var created = await _questionRepository.GetByIdAsync(question.Id);
        return CreatedAtAction(nameof(GetById), new { id = question.Id }, created!.ToDto());
    }

    //PUT api/questions/3
    [HttpPut("{id:int}")]
    public async Task<ActionResult<QuestionDto>> Update(int id, SaveQuestionDto dto)
    {
        if (await _schoolRepository.GetByIdAsync(dto.SchoolId) == null)
            return SchoolDoesNotExist();

        if (!await _questionRepository.UpdateAsync(dto.ToModel(id)))
        {
            _logger.LogWarning("Tried to update question {QuestionId}, which does not exist", id);
            return NotFound();
        }

        _logger.LogInformation("Question {QuestionId} updated", id);
        var updated = await _questionRepository.GetByIdAsync(id);
        return Ok(updated!.ToDto());
    }

    //DELETE api/questions/3
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _questionRepository.DeleteAsync(id))
        {
            _logger.LogWarning("Tried to delete question {QuestionId}, which does not exist", id);
            return NotFound();
        }

        _logger.LogInformation("Question {QuestionId} deleted", id);
        return NoContent();
    }

    //400 with the error at the SchoolId field, same format as the other validation errors
    private ActionResult SchoolDoesNotExist()
    {
        ModelState.AddModelError(nameof(SaveQuestionDto.SchoolId), "Skolen finnes ikke.");
        return ValidationProblem(ModelState);
    }
}

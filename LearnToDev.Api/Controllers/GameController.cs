using System.Security.Claims;
using LearnToDev.Api.DTOs;
using LearnToDev.Api.Repositories.Interfaces;
using LearnToDev.Api.Services;
using LearnToDev.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnToDev.Api.Controllers;

//The game for the logged in user: status, exams and jobs. The rules are in ExamService and JobService (R-03).
//The user is always taken from the login cookie, so a player can only change their own game
[ApiController]
[Route("api/game")]
[Authorize]
public class GameController : ControllerBase
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IExamService _examService;
    private readonly IJobService _jobService;

    public GameController(IPlayerRepository playerRepository, ISchoolRepository schoolRepository,
        IExamService examService, IJobService jobService)
    {
        _playerRepository = playerRepository;
        _schoolRepository = schoolRepository;
        _examService = examService;
        _jobService = jobService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    //GET api/game/status
    [HttpGet("status")]
    public async Task<ActionResult<PlayerStatusDto>> GetStatus()
    {
        var player = await _playerRepository.GetOrCreateAsync(UserId);
        var passed = player.PassedExams.Select(e => e.School.Name).OrderBy(n => n).ToList();
        return Ok(new PlayerStatusDto(player.CurrentJob?.Name, passed));
    }

    //GET api/game/schools: the schools, and which ones the player has passed
    [HttpGet("schools")]
    public async Task<ActionResult<List<PlayerSchoolDto>>> GetSchools()
    {
        var player = await _playerRepository.GetOrCreateAsync(UserId);
        var schools = await _schoolRepository.GetAllAsync();
        return Ok(schools.Select(s => new PlayerSchoolDto(s.Id, s.Name, s.Description, player.HasPassed(s.Id))).ToList());
    }

    //GET api/game/exams/3: draws a new exam
    [HttpGet("exams/{schoolId:int}")]
    public async Task<ActionResult<ExamDto>> StartExam(int schoolId)
    {
        return ToActionResult(await _examService.StartExamAsync(schoolId));
    }

    //POST api/game/exams/3: hands in the answers and gets the result
    [HttpPost("exams/{schoolId:int}")]
    public async Task<ActionResult<ExamResultDto>> SubmitExam(int schoolId, SubmitExamDto dto)
    {
        return ToActionResult(await _examService.SubmitExamAsync(UserId, schoolId, dto));
    }

    //GET api/game/jobs: every job, open or locked
    [HttpGet("jobs")]
    public async Task<ActionResult<List<PlayerJobDto>>> GetJobs()
    {
        return Ok(await _jobService.GetJobsForPlayerAsync(UserId));
    }

    //POST api/game/jobs/3/take
    [HttpPost("jobs/{jobId:int}/take")]
    public async Task<ActionResult<PlayerJobDto>> TakeJob(int jobId)
    {
        return ToActionResult(await _jobService.TakeJobAsync(UserId, jobId));
    }

    //POST api/game/jobs/quit
    [HttpPost("jobs/quit")]
    public async Task<IActionResult> QuitJob()
    {
        var result = await _jobService.QuitJobAsync(UserId);
        if (result.Status != ServiceStatus.Success)
            return ToActionResult(result).Result!;

        return NoContent();
    }

    //Turns the service result into a status code with ProblemDetails, so React can show the message
    private ActionResult<T> ToActionResult<T>(ServiceResult<T> result)
    {
        return result.Status switch
        {
            ServiceStatus.Success => Ok(result.Value),
            ServiceStatus.NotFound => Problem(detail: result.Message, statusCode: StatusCodes.Status404NotFound),
            ServiceStatus.Invalid => Problem(detail: result.Message, statusCode: StatusCodes.Status400BadRequest),
            _ => Problem(detail: result.Message, statusCode: StatusCodes.Status409Conflict)
        };
    }
}

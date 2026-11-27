using System.Security.Claims;
using LearnToDev.Api.DTOs;
using LearnToDev.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearnToDev.Api.Services;
using LearnToDev.Api.Services.Interfaces;

namespace LearnToDev.Api.Controllers;

//Spillet for den innloggede brukeren. Alle endepunktene krever innlogging
[ApiController]
[Route("api/game")]
[Authorize]
public class GameController : ControllerBase
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<GameController> _logger;
    private readonly IExamService _examService;
    private readonly IJobService _jobService;

    public GameController(IPlayerRepository playerRepository, ILogger<GameController> logger, IExamService examService, IJobService jobService)
    {
        _playerRepository = playerRepository;
        _logger = logger;
        _examService = examService;
        _jobService = jobService;
    }

    //Startskjermen: nåværende jobb og beståtte eksamener
    [HttpGet("status")]
    public async Task<ActionResult<PlayerStatusDto>> GetStatus()
    {
        //Id-en kommer fra innloggingscookien, ikke fra URL-en, så ingen kan hente andres spill
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        //Første gang brukeren åpner spillet, lages spilleren her
        var player = await _playerRepository.GetOrCreateAsync(userId);
        _logger.LogDebug("Spiller {PlayerId} hentet status.", player.Id);

        //Sender bare det startskjermen trenger. Skolene sorteres, så rekkefølgen alltid er den samme
        return Ok(new PlayerStatusDto
        {
            CurrentJobName = player.CurrentJob?.Name,
            PassedSchools = player.PassedExams
                .Select(e => e.School.Name)
                .OrderBy(name => name)
                .ToList()
        });
    }

    //Trekker en ny eksamen fra skolen. Svarene sendes uten IsCorrect, så fasiten ikke lekker
    [HttpGet("exams/{schoolId:int}")]
    public async Task<ActionResult<ExamDto>> StartExam(int schoolId)
    {
        var result = await _examService.StartExamAsync(schoolId);
        return ToActionResult(result);
    }

    //Retter eksamen og lagrer den som bestått hvis spilleren fikk nok riktige
    [HttpPost("exams/{schoolId:int}")]
    public async Task<ActionResult<ExamResultDto>> SubmitExam(int schoolId, SubmitExamDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _examService.SubmitExamAsync(userId, schoolId, dto);
        return ToActionResult(result);
    }

    //Skoleoversikten: alle skolene, med om spilleren har bestått eksamen i hver av dem
    [HttpGet("schools")]
    public async Task<ActionResult<List<PlayerSchoolDto>>> GetSchools()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        return Ok(await _examService.GetSchoolsAsync(userId));
    }

    //Jobbsiden: alle jobbene, med om de er åpne eller låste, og hvorfor
    [HttpGet("jobs")]
    public async Task<ActionResult<List<JobEligibility>>> GetJobs()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        return Ok(await _jobService.CheckAllJobsAsync(userId));
    }

    //Tar en jobb. En låst jobb gir 409 med årsaken
    [HttpPost("jobs/{jobId:int}/take")]
    public async Task<ActionResult<JobEligibility>> TakeJob(int jobId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        return ToActionResult(await _jobService.TakeJobAsync(userId, jobId));
    }

    //Sier opp jobben. Uten jobb gir det 409
    [HttpPost("jobs/quit")]
    public async Task<IActionResult> QuitJob()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _jobService.QuitJobAsync(userId);
        if (result.Status != ServiceStatus.Success)
        {
            return ToActionResult(result).Result!;
        }

        //204: det gikk bra, men det er ingenting å sende tilbake
        return NoContent();
    }

    //Gjør om utfallet fra servicen til riktig statuskode. Feilene sendes som ProblemDetails
    private ActionResult<T> ToActionResult<T>(ServiceResult<T> result)
    {
        return result.Status switch
        {
            ServiceStatus.Success => Ok(result.Value),
            ServiceStatus.NotFound => Problem(detail: result.Message, statusCode: StatusCodes.Status404NotFound),
            ServiceStatus.Invalid => Problem(detail: result.Message, statusCode: StatusCodes.Status400BadRequest),
            ServiceStatus.Conflict => Problem(detail: result.Message, statusCode: StatusCodes.Status409Conflict),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "Ukjent ServiceStatus.")
        };
    }
}

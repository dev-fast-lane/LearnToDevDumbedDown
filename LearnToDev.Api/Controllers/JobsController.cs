using LearnToDev.Api.DTOs;
using LearnToDev.Api.Models;
using LearnToDev.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnToDev.Api.Controllers;

//CRUD for jobs and the schools they require (R-01, R-02). Admin only
[ApiController]
[Route("api/jobs")]
[Authorize(Roles = Roles.Admin)]
public class JobsController : ControllerBase
{
    private readonly IJobRepository _jobRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IJobRepository jobRepository, ISchoolRepository schoolRepository, ILogger<JobsController> logger)
    {
        _jobRepository = jobRepository;
        _schoolRepository = schoolRepository;
        _logger = logger;
    }

    //GET api/jobs
    [HttpGet]
    public async Task<ActionResult<List<JobDto>>> GetAll()
    {
        var jobs = await _jobRepository.GetAllAsync();
        return Ok(jobs.Select(j => j.ToDto()).ToList());
    }

    //GET api/jobs/3
    [HttpGet("{id:int}")]
    public async Task<ActionResult<JobDto>> GetById(int id)
    {
        var job = await _jobRepository.GetByIdAsync(id);
        if (job == null)
            return NotFound();

        return Ok(job.ToDto());
    }

    //POST api/jobs
    [HttpPost]
    public async Task<ActionResult<JobDto>> Create(SaveJobDto dto)
    {
        if (!await AllSchoolsExistAsync(dto.RequiredSchoolIds))
            return SchoolDoesNotExist();

        var job = await _jobRepository.AddAsync(dto.ToModel());
        _logger.LogInformation("Job {JobId} created", job.Id);

        var created = await _jobRepository.GetByIdAsync(job.Id);
        return CreatedAtAction(nameof(GetById), new { id = job.Id }, created!.ToDto());
    }

    //PUT api/jobs/3
    [HttpPut("{id:int}")]
    public async Task<ActionResult<JobDto>> Update(int id, SaveJobDto dto)
    {
        if (!await AllSchoolsExistAsync(dto.RequiredSchoolIds))
            return SchoolDoesNotExist();

        if (!await _jobRepository.UpdateAsync(dto.ToModel(id)))
        {
            _logger.LogWarning("Tried to update job {JobId}, which does not exist", id);
            return NotFound();
        }

        _logger.LogInformation("Job {JobId} updated", id);
        var updated = await _jobRepository.GetByIdAsync(id);
        return Ok(updated!.ToDto());
    }

    //DELETE api/jobs/3
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _jobRepository.DeleteAsync(id))
        {
            _logger.LogWarning("Tried to delete job {JobId}, which does not exist", id);
            return NotFound();
        }

        _logger.LogInformation("Job {JobId} deleted", id);
        return NoContent();
    }

    private async Task<bool> AllSchoolsExistAsync(List<int> schoolIds)
    {
        var existing = (await _schoolRepository.GetAllAsync()).Select(s => s.Id).ToHashSet();
        return schoolIds.All(existing.Contains);
    }

    private ActionResult SchoolDoesNotExist()
    {
        ModelState.AddModelError(nameof(SaveJobDto.RequiredSchoolIds), "En av skolene finnes ikke.");
        return ValidationProblem(ModelState);
    }
}

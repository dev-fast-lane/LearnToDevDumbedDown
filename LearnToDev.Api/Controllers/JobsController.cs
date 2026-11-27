using LearnToDev.Api.DTOs;
using LearnToDev.Api.Models;
using LearnToDev.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnToDev.Api.Controllers;

//CRUD for jobber med skolekrav. Alle innloggede kan lese, bare admin kan endre
[ApiController]
[Route("api/jobs")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly IJobRepository _jobRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IJobRepository jobRepository, ISchoolRepository schoolRepository,
        ILogger<JobsController> logger)
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
        return Ok(jobs.Select(ToDto).ToList());
    }

    //GET api/jobs/3
    [HttpGet("{id:int}")]
    public async Task<ActionResult<JobDto>> GetById(int id)
    {
        var job = await _jobRepository.GetByIdAsync(id);
        if (job == null)
        {
            return NotFound();
        }

        return Ok(ToDto(job));
    }
    
    //POST api/jobs
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<JobDto>> Create(SaveJobDto dto)
    {
        if (!await AllSchoolsExistAsync(dto.RequiredSchoolIds))
        {
            return ValidationProblem(ModelState);
        }

        var job = new Job
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            JobRequirements = ToRequirements(dto.RequiredSchoolIds)
        };

        await _jobRepository.AddAsync(job);
        _logger.LogInformation("Jobb {JobId} opprettet med {RequirementCount} skolekrav.",
            job.Id, job.JobRequirements.Count);

        //Henter jobben på nytt, så skolenavnene kommer med i svaret
        var created = await _jobRepository.GetByIdAsync(job.Id);
        return CreatedAtAction(nameof(GetById), new { id = job.Id }, ToDto(created!));
    }

    //PUT api/jobs/3. Erstatter navn, beskrivelse og alle skolekravene
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<JobDto>> Update(int id, SaveJobDto dto)
    {
        if (!await AllSchoolsExistAsync(dto.RequiredSchoolIds))
        {
            return ValidationProblem(ModelState);
        }

        var job = new Job
        {
            Id = id,
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            JobRequirements = ToRequirements(dto.RequiredSchoolIds)
        };

        if (!await _jobRepository.UpdateAsync(job))
        {
            _logger.LogWarning("Prøvde å oppdatere jobb {JobId}, som ikke finnes.", id);
            return NotFound();
        }

        _logger.LogInformation("Jobb {JobId} oppdatert med {RequirementCount} skolekrav.",
            id, job.JobRequirements.Count);

        var updated = await _jobRepository.GetByIdAsync(id);
        return Ok(ToDto(updated!));
    }

    //DELETE api/jobs/3. Spillere med denne jobben blir arbeidsledige
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _jobRepository.DeleteAsync(id))
        {
            _logger.LogWarning("Prøvde å slette jobb {JobId}, som ikke finnes.", id);
            return NotFound();
        }

        _logger.LogInformation("Jobb {JobId} slettet.", id);
        return NoContent();
    }
    
    // HJELPEMETODER
    // _________________________________________________________

    //Legger til en valideringsfeil og returnerer false hvis en av skolene ikke finnes
    private async Task<bool> AllSchoolsExistAsync(List<int> schoolIds)
    {
        var existingIds = (await _schoolRepository.GetAllAsync()).Select(s => s.Id);
        var missing = schoolIds.Except(existingIds).ToList();
        if (missing.Count == 0)
        {
            return true;
        }

        _logger.LogWarning("Avviste jobb med ukjente skoler {SchoolIds}.", missing);
        ModelState.AddModelError(nameof(SaveJobDto.RequiredSchoolIds),
            $"Fant ikke skole med id {string.Join(", ", missing)}.");
        return false;
    }

    //Distinct fjerner duplikater, siden samme skole bare kan være krav én gang per jobb
    private static List<JobRequirement> ToRequirements(List<int> schoolIds)
    {
        return schoolIds.Distinct().Select(id => new JobRequirement { SchoolId = id }).ToList();
    }

    //Gjør om en Job til DTO-en vi sender ut, så entiteten aldri forlater API-et (R-09)
    private static JobDto ToDto(Job job)
    {
        var schools = job.JobRequirements
            .Select(r => new SchoolDto(r.School.Id, r.School.Name, r.School.Description))
            .OrderBy(s => s.Name)
            .ToList();

        return new JobDto(job.Id, job.Name, job.Description, schools);
    }
}
using LearnToDev.Api.DTOs;
using LearnToDev.Api.Models;
using LearnToDev.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnToDev.Api.Controllers;

//CRUD for schools. Everyone logged in can read, only admins can change (R-01, R-11)
[ApiController]
[Route("api/schools")]
[Authorize]
public class SchoolsController : ControllerBase
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger<SchoolsController> _logger;

    public SchoolsController(ISchoolRepository schoolRepository, ILogger<SchoolsController> logger)
    {
        _schoolRepository = schoolRepository;
        _logger = logger;
    }

    //GET api/schools
    [HttpGet]
    public async Task<ActionResult<List<SchoolDto>>> GetAll()
    {
        var schools = await _schoolRepository.GetAllAsync();
        return Ok(schools.Select(s => s.ToDto()).ToList());
    }

    //GET api/schools/3
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SchoolDto>> GetById(int id)
    {
        var school = await _schoolRepository.GetByIdAsync(id);
        if (school == null)
            return NotFound();

        return Ok(school.ToDto());
    }

    //POST api/schools
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<SchoolDto>> Create(SaveSchoolDto dto)
    {
        var school = await _schoolRepository.AddAsync(dto.ToModel());
        _logger.LogInformation("School {SchoolId} created", school.Id);

        //201 Created, with a link to the new school in the Location header
        return CreatedAtAction(nameof(GetById), new { id = school.Id }, school.ToDto());
    }

    //PUT api/schools/3
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<SchoolDto>> Update(int id, SaveSchoolDto dto)
    {
        var school = dto.ToModel(id);
        if (!await _schoolRepository.UpdateAsync(school))
        {
            _logger.LogWarning("Tried to update school {SchoolId}, which does not exist", id);
            return NotFound();
        }

        _logger.LogInformation("School {SchoolId} updated", id);
        return Ok(school.ToDto());
    }

    //DELETE api/schools/3
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _schoolRepository.DeleteAsync(id))
        {
            _logger.LogWarning("Tried to delete school {SchoolId}, which does not exist", id);
            return NotFound();
        }

        _logger.LogInformation("School {SchoolId} deleted", id);
        return NoContent();
    }
}

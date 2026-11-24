using LearnToDev.Api.DTOs;
using LearnToDev.Api.Repositories.Interfaces;
using LearnToDev.Api.Models;
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
        return Ok(schools.Select(ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SchoolDto>> GetById(int id)
    {
        var school = await _schoolRepository.GetByIdAsync(id);
        if (school == null)
        {
            return NotFound();
        }
        return Ok(ToDto(school));
    }

    //POST api/schools/3
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<SchoolDto>> Create(SaveSchoolDto dto)
    {
        var school = new School
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim()
        };

        await _schoolRepository.AddAsync(school);
        _logger.LogInformation("School {SchoolId} created", school.Id);

        //201 Created, with a link to the new school in the Location header
        return CreatedAtAction(nameof(GetById), new { id = school.Id }, ToDto(school));
    }

    //PUT api/schools/3
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<SchoolDto>> Update(int id, SaveSchoolDto dto)
    {
        var school = new School
        {
            Id = id,
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim()
        };

        if (!await _schoolRepository.UpdateAsync(school))
        {
            _logger.LogWarning("Tried to update school {SchoolId}, which does not exist", id);
            return NotFound();
        }

        _logger.LogInformation("School {SchoolId} updated", id);
        return Ok(ToDto(school));
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

    //Turns a School into the DTO we send out, so the entity never leaves the API (R-09)
    private static SchoolDto ToDto(School school) => new(school.Id, school.Name, school.Description);
}